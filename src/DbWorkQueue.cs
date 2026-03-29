using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiteDB;

internal sealed class DbWorkQueue
{
    private readonly Channel<DbWork> queue = Channel.CreateBounded<DbWork>( new BoundedChannelOptions( Environment.ProcessorCount * 4 )
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false,
    } );

    public async Task InvokeAsync( Func<CancellationToken, ValueTask> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( work );

        var completion = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
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

        await using( cancellation.Register( ( ) => completion.TrySetCanceled( cancellation ) ) )
        {
            await completion.Task.ConfigureAwait( false );
        }
    }

    public async Task<T> InvokeAsync<T>( Func<CancellationToken, ValueTask<T>> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( work );

        var completion = new TaskCompletionSource<T>( TaskCreationOptions.RunContinuationsAsynchronously );
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

        await using( cancellation.Register( ( ) => completion.TrySetCanceled( cancellation ) ) )
        {
            return await completion.Task.ConfigureAwait( false );
        }
    }

    public ValueTask<DbWork> ReadAsync( CancellationToken cancellation ) => queue.Reader.ReadAsync( cancellation );
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
    public static async IAsyncEnumerable<T> EnumerateAsync<T>( this DbWorkQueue queue, Func<IEnumerable<T>> factory, [EnumeratorCancellation] CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );
        ArgumentNullException.ThrowIfNull( factory );

        var channel = Channel.CreateBounded<T>( new BoundedChannelOptions( Environment.ProcessorCount * 2 )
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        } );

        var writer = queue.InvokeAsync( async cancellation =>
        {
            foreach( var value in factory() )
            {
                await channel.Writer.WriteAsync( value, cancellation );
            }
        }, cancellation ).ContinueWith( _ => channel.Writer.Complete( _.Exception ), CancellationToken.None );

        await foreach( var value in channel.Reader.ReadAllAsync( cancellation ).ConfigureAwait( false ) )
        {
            yield return value;

            if( writer.IsFaulted )
            {
                break;
            }
        }

        await writer.ConfigureAwait( false );
    }

    public static Task InvokeAsync( this DbWorkQueue queue, Action work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );
        ArgumentNullException.ThrowIfNull( work );

        return queue.InvokeAsync( _ =>
        {
            work();
            return default;
        }, cancellation );
    }

    public static Task<T> InvokeAsync<T>( this DbWorkQueue queue, Func<T> work, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( queue );
        ArgumentNullException.ThrowIfNull( work );

        return queue.InvokeAsync<T>( _ => new( work() ), cancellation );
    }
}