using System.Runtime.CompilerServices;

namespace LiteDB;

public abstract class LiteDbContext : IAsyncDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly LiteDatabase database;
    private readonly DbWorkQueue queue;

    public BsonMapper Mapper => database.Mapper;

    protected LiteDbContext( LiteDbOptions options )
    {
        queue = new();
        database = new( options.ConnectionString, options.Mapper )
        {
            UserVersion = options.UserVersion,
            UtcDate = true,
        };

        _ = ProcessWorkQueue( queue, cancellation.Token );
    }

    protected LiteDbSet<T> DbSet<T>( [CallerMemberName] string? name = default ) => new(
        database.GetCollection<T>( name ),
        queue );

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