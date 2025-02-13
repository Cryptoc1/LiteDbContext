using System.Threading.Channels;

namespace LiteDB;

internal sealed class DbWorkQueue
{
    private readonly Channel<DbWork> queue = Channel.CreateUnbounded<DbWork>( new()
    {
        SingleReader = true
    } );

    public async Task InvokeAsync( Func<CancellationToken, ValueTask> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( work );

        var completion = new TaskCompletionSource();
        await queue.WriteAsync( async cancellation =>
        {
            try
            {
                await work( cancellation ).ConfigureAwait( false );
                completion.SetResult();
            }
            catch( Exception e )
            {
                completion.TrySetException( e );
            }
        }, cancellation );

        using( cancellation.Register( ( ) => completion.TrySetCanceled( cancellation ) ) )
        {
            await completion.Task.ConfigureAwait( false );
        }
    }

    public async Task<T> InvokeAsync<T>( Func<CancellationToken, ValueTask<T>> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( work );

        var completion = new TaskCompletionSource<T>();
        await queue.WriteAsync( async cancellation =>
        {
            try
            {
                var result = await work( cancellation ).ConfigureAwait( false );
                completion.SetResult( result );
            }
            catch( Exception e )
            {
                completion.TrySetException( e );
            }
        }, cancellation );

        using( cancellation.Register( ( ) => completion.TrySetCanceled( cancellation ) ) )
        {
            return await completion.Task.ConfigureAwait( false );
        }
    }

    public ValueTask<DbWork> ReadAsync( CancellationToken cancellation ) => queue.Reader.ReadAsync( cancellation );
    public ValueTask WriteAsync( DbWork work, CancellationToken cancellation ) => queue.Writer.WriteAsync( work, cancellation );
}

internal sealed class DbWork( Func<CancellationToken, ValueTask> work, CancellationToken cancellation )
{
    private readonly CancellationToken cancellation = cancellation;

    public async ValueTask Invoke( CancellationToken cancellation )
    {
        using var combined = CancellationTokenSource.CreateLinkedTokenSource( this.cancellation, cancellation );
        await work( combined.Token ).ConfigureAwait( false );
    }
}

internal static class DbWorkChannelExtensions
{
    public static ValueTask WriteAsync( this Channel<DbWork> channel, Func<CancellationToken, ValueTask> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( channel );
        ArgumentNullException.ThrowIfNull( work );

        return channel.Writer.WriteAsync( new( work, cancellation ), cancellation );
    }
}

internal static class DbWorkQueueExtensions
{
    public static IAsyncEnumerable<T> EnumerateAsync<T>( this DbWorkQueue queue, Func<IEnumerable<T>> factory, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );
        ArgumentNullException.ThrowIfNull( factory );

        var channel = Channel.CreateUnbounded<T>( new()
        {
            SingleReader = true,
            SingleWriter = true,
        } );

        queue.InvokeAsync( async cancellation =>
        {
            foreach( var value in factory() )
            {
                await channel.Writer.WriteAsync( value, cancellation );
                cancellation.ThrowIfCancellationRequested();
            }
        }, cancellation ).ContinueWith( _ => channel.Writer.Complete( _.Exception ), CancellationToken.None );

        return channel.Reader.ReadAllAsync( cancellation );
    }

    public static Task InvokeAsync( this DbWorkQueue queue, Action work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );
        ArgumentNullException.ThrowIfNull( work );

        return queue.InvokeAsync( _ =>
        {
            work();
            return ValueTask.CompletedTask;
        }, cancellation );
    }

    public static Task<T> InvokeAsync<T>( this DbWorkQueue queue, Func<T> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );
        ArgumentNullException.ThrowIfNull( work );

        return queue.InvokeAsync( _ => ValueTask.FromResult( work() ), cancellation );
    }
}