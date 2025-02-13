using System.Runtime.CompilerServices;

namespace LiteDB;

public abstract class LiteDbContext : IAsyncDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly LiteDatabase database;
    private readonly DbWorkQueue queue;

    protected LiteDbContext( LiteDbOptions options )
    {
        queue = new();
        database = new( options.ConnectionString, options.Mapper );

        _ = ProcessWorkQueue( queue, cancellation.Token );
    }

    protected LiteDbSetBuilder<T> DbSet<T>( [CallerMemberName] string? name = default ) => new( database, name, queue );

    public async ValueTask DisposeAsync( )
    {
        await DisposeAsyncCore();
        if( !cancellation.IsCancellationRequested )
        {
            await cancellation.CancelAsync();
        }

        cancellation.Dispose();
        database.Dispose();

        GC.SuppressFinalize( this );
    }

    protected virtual ValueTask DisposeAsyncCore( ) => default;

    private static async Task ProcessWorkQueue( DbWorkQueue queue, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );

        while( !cancellation.IsCancellationRequested )
        {
            var work = await queue.ReadAsync( cancellation ).ConfigureAwait( false );
            if( work is not null )
            {
                await work.Invoke( cancellation ).ConfigureAwait( false );
            }
        }
    }
}

public sealed class LiteDbSetBuilder<T>
{
    private readonly ILiteDatabase database;
    private readonly DbWorkQueue queue;

    private string? name;
    private Action<ILiteCollection<T>>? onCreating;

    internal LiteDbSetBuilder( ILiteDatabase database, string? name, DbWorkQueue queue )
    {
        this.database = database;
        this.queue = queue;

        this.name = name;
    }

    public LiteDbSet<T> Build( )
    {
        var collection = database.GetCollection<T>( name );
        onCreating?.Invoke( collection );

        return new LiteDbSet<T>( collection, queue );
    }

    public LiteDbSetBuilder<T> OnCreatingCollection( Action<ILiteCollection<T>> onCreating )
    {
        ArgumentNullException.ThrowIfNull( onCreating );

        this.onCreating = onCreating;
        return this;
    }

    public LiteDbSetBuilder<T> WithName( string name )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace( name );

        this.name = name;
        return this;
    }

    public static implicit operator LiteDbSet<T>( LiteDbSetBuilder<T> builder ) => builder.Build();
}