using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace LiteDB;

internal sealed class LiteDbResult<T>( ILiteQueryableResult<T> result, DbWorkQueue queue ) : ILiteDbResult<T>
{
    public ILiteDbResult<T> Limit( int limit ) => new LiteDbResult<T>(
        result.Limit( limit ),
        queue );

    public ILiteDbResult<T> Skip( int offset ) => new LiteDbResult<T>(
        result.Skip( offset ),
        queue );

    public ILiteDbResult<T> Offset( int offset ) => new LiteDbResult<T>(
        result.Offset( offset ),
        queue );

    public ILiteDbResult<T> ForUpdate( ) => new LiteDbResult<T>(
        result.ForUpdate(),
        queue );

    public Task<BsonDocument> GetPlanAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.GetPlan(),
        cancellation );

    public ChannelReader<BsonValue> ExecuteReader( CancellationToken cancellation = default )
    {
        var channel = Channel.CreateUnbounded<BsonValue>( new()
        {
            SingleWriter = true,
        } );

        queue.InvokeAsync( async cancellation =>
        {
            using var reader = result.ExecuteReader();
            while( reader.Read() )
            {
                await channel.Writer.WriteAsync( reader.Current, cancellation );
            }
        }, cancellation ).ContinueWith( _ => channel.Writer.Complete( _.Exception ), CancellationToken.None );

        return channel.Reader;
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable( CancellationToken cancellation = default ) => queue.EnumerateAsync(
        result.ToEnumerable,
        cancellation );

    public Task<int> IntoAsync( string collection, BsonAutoId autoId = BsonAutoId.ObjectId, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.Into( collection, autoId ),
        cancellation );

    public Task<T> FirstAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.First(),
        cancellation );

    public Task<T?> FirstOrDefaultAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.FirstOrDefault(),
        cancellation )!;

    public Task<T> SingleAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.Single(),
        cancellation );

    public Task<T?> SingleOrDefaultAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.SingleOrDefault(),
        cancellation )!;

    public Task<int> CountAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.Count(),
        cancellation );

    public Task<long> LongCountAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.LongCount(),
        cancellation );

    public Task<bool> ExistsAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => result.Exists(),
        cancellation );
}

public interface ILiteDbResult<T>
{
    public ILiteDbResult<T> Limit( int limit );
    public ILiteDbResult<T> Skip( int offset );
    public ILiteDbResult<T> Offset( int offset );
    public ILiteDbResult<T> ForUpdate( );

    public Task<BsonDocument> GetPlanAsync( CancellationToken cancellation = default );

    public ChannelReader<BsonValue> ExecuteReader( CancellationToken cancellation = default );
    public IAsyncEnumerable<T> ToAsyncEnumerable( CancellationToken cancellation = default );

    public Task<int> IntoAsync( string collection, BsonAutoId autoId = BsonAutoId.ObjectId, CancellationToken cancellation = default );

    public Task<T> FirstAsync( CancellationToken cancellation = default );
    public Task<T?> FirstOrDefaultAsync( CancellationToken cancellation = default );

    public Task<T> SingleAsync( CancellationToken cancellation = default );
    public Task<T?> SingleOrDefaultAsync( CancellationToken cancellation = default );

    public Task<int> CountAsync( CancellationToken cancellation = default );
    public Task<long> LongCountAsync( CancellationToken cancellation = default );

    public Task<bool> ExistsAsync( CancellationToken cancellation = default );
}

public static class LiteDbResultExtensions
{
    public static async Task<T[]> ToArrayAsync<T>( this ILiteDbResult<T> result, CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( result );

        var buffer = ArrayPool<T>.Shared.Rent( 10 );
        var count = 0;

        await foreach( var value in result.ToAsyncEnumerable( cancellation ) )
        {
            if( count == buffer.Length )
            {
                Resize( ref buffer, count );
            }

            buffer[ count++ ] = value;
        }

        var values = new T[ count ];
        Array.Copy( buffer, values, count );

        ArrayPool<T>.Shared.Return( buffer );
        return values;

        static void Resize( ref T[] buffer, int count )
        {
            var resized = ArrayPool<T>.Shared.Rent( buffer.Length + (buffer.Length >> 1) );
            Array.Copy( buffer, resized, count );

            ArrayPool<T>.Shared.Return( buffer );
            buffer = resized;
        }
    }

    public static async IAsyncEnumerable<BsonDocument> ToDocumentsAsync<T>( this ILiteDbResult<T> result, [EnumeratorCancellation] CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( result );

        await foreach( var value in result.ExecuteReader( cancellation ).ReadAllAsync( cancellation ) )
        {
            yield return value.AsDocument;
        }
    }

    public static async Task<List<T>> ToListAsync<T>( this ILiteDbResult<T> result, CancellationToken cancellation = default )
    {
        ArgumentNullException.ThrowIfNull( result );

        var values = new List<T>();
        await foreach( var value in result.ToAsyncEnumerable( cancellation ) )
        {
            values.Add( value );
        }

        return values;
    }
}