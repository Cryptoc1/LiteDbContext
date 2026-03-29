using System.Runtime.CompilerServices;

namespace LiteDB;

public abstract class LiteDbContext : IAsyncDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly LiteDatabase database;
    private readonly DbWorkQueue queue;

    private bool disposed;
    private Task? worker;

    public BsonMapper Mapper => database.Mapper;

    protected LiteDbContext( LiteDbOptions options )
    {
        ArgumentNullException.ThrowIfNull( options );

        database = CreateDatabase( options );
        queue = new();
        worker = Task.Run( ( ) => ProcessWorkQueue( queue, cancellation.Token ), cancellation.Token );
    }

    private LiteDatabase CreateDatabase( LiteDbOptions options )
    {
        var mapper = new BsonMapper()
        {
            EnumAsInteger = true
        };

        mapper.ConfigureDates()
            .ConfigureJson()
            .ConfigureTime()
            .ConfigureUris();

        OnCreatingMapper( mapper );
        var database = new LiteDatabase( options.ConnectionString, mapper )
        {
            UtcDate = true,
        };

        OnCreatingDatabase( database );
        return database;
    }

    protected LiteDbSet<T> DbSet<T>( [CallerMemberName] string? name = default )
    {
        ObjectDisposedException.ThrowIf( disposed, this );

        var collection = database.GetCollection<T>( name );
        return new( collection, queue );
    }

    public async ValueTask DisposeAsync( )
    {
        disposed = true;

        await DisposeAsyncCore();
        if( !cancellation.IsCancellationRequested )
        {
            await cancellation.CancelAsync();
        }

        if( worker is not null )
        {
            try
            {
                await worker.ConfigureAwait( false );
            }
            catch( OperationCanceledException e ) when( e.CancellationToken == cancellation.Token ) { }
            finally
            {
                worker = default;
            }
        }

        cancellation.Dispose();
        database.Dispose();

        GC.SuppressFinalize( this );
    }

    protected virtual ValueTask DisposeAsyncCore( ) => default;

    protected virtual void OnCreatingDatabase( LiteDatabase database )
    {
    }

    protected virtual void OnCreatingMapper( BsonMapper mapper )
    {
    }

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