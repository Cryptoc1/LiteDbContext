using System.Linq.Expressions;
using System.Threading.Channels;

namespace LiteDB;

internal sealed class LiteDbQueryable<T>( ILiteQueryable<T> query, DbWorkQueue queue ) : ILiteDbQueryable<T>
{
    public ILiteDbQueryable<T> Include( BsonExpression path ) => new LiteDbQueryable<T>(
        query.Include( path ),
        queue );

    public ILiteDbQueryable<T> Include( List<BsonExpression> paths ) => new LiteDbQueryable<T>(
        query.Include( paths ),
        queue );

    public ILiteDbQueryable<T> Include<TKey>( Expression<Func<T, TKey>> path ) => new LiteDbQueryable<T>(
        query.Include( path ),
        queue );

    public ILiteDbQueryable<T> Where( BsonExpression predicate ) => new LiteDbQueryable<T>(
        query.Where( predicate ),
        queue );

    public ILiteDbQueryable<T> Where( string predicate, BsonDocument parameters ) => new LiteDbQueryable<T>(
        query.Where( predicate, parameters ),
        queue );

    public ILiteDbQueryable<T> Where( string predicate, params BsonValue[] args ) => new LiteDbQueryable<T>(
        query.Where( predicate, args ),
        queue );

    public ILiteDbQueryable<T> Where( Expression<Func<T, bool>> predicate ) => new LiteDbQueryable<T>(
        query.Where( predicate ),
        queue );

    public ILiteDbQueryable<T> OrderBy( BsonExpression selector, int order ) => new LiteDbQueryable<T>(
        query.OrderBy( selector, order ),
        queue );

    public ILiteDbQueryable<T> OrderBy<TKey>( Expression<Func<T, TKey>> selector, int order ) => new LiteDbQueryable<T>(
        query.OrderBy( selector, order ),
        queue );

    public ILiteDbQueryable<T> OrderByDescending( BsonExpression selector ) => new LiteDbQueryable<T>(
        query.OrderByDescending( selector ),
        queue );

    public ILiteDbQueryable<T> OrderByDescending<TKey>( Expression<Func<T, TKey>> selector ) => new LiteDbQueryable<T>(
        query.OrderByDescending( selector ),
        queue );

    public ILiteDbQueryable<T> GroupBy( BsonExpression selector ) => new LiteDbQueryable<T>(
        query.GroupBy( selector ),
        queue );

    public ILiteDbQueryable<T> Having( BsonExpression predicate ) => new LiteDbQueryable<T>(
        query.Having( predicate ),
        queue );

    public ILiteDbResult<BsonDocument> Select( BsonExpression selector ) => new LiteDbResult<BsonDocument>(
        query.Select( selector ),
        queue );

    public ILiteDbResult<TResult> Select<TResult>( Expression<Func<T, TResult>> selector ) => new LiteDbResult<TResult>(
        query.Select( selector ),
        queue );

    public ILiteDbResult<T> Limit( int limit ) => new LiteDbResult<T>( query, queue ).Limit( limit );
    public ILiteDbResult<T> Skip( int offset ) => new LiteDbResult<T>( query, queue ).Skip( offset );
    public ILiteDbResult<T> Offset( int offset ) => new LiteDbResult<T>( query, queue ).Offset( offset );
    public ILiteDbResult<T> ForUpdate( ) => new LiteDbResult<T>( query, queue ).ForUpdate();

    public Task<BsonDocument> GetPlanAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).GetPlanAsync( cancellation );
    public ChannelReader<BsonValue> ExecuteReader( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).ExecuteReader( cancellation );
    public IAsyncEnumerable<T> ToAsyncEnumerable( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).ToAsyncEnumerable( cancellation );
    public Task<int> IntoAsync( string collection, BsonAutoId autoId = BsonAutoId.ObjectId, CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).IntoAsync( collection, autoId, cancellation );
    public Task<T> FirstAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).FirstAsync( cancellation );
    public Task<T?> FirstOrDefaultAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).FirstOrDefaultAsync( cancellation );
    public Task<T> SingleAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).SingleAsync( cancellation );
    public Task<T?> SingleOrDefaultAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).SingleOrDefaultAsync( cancellation );
    public Task<int> CountAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).CountAsync( cancellation );
    public Task<long> LongCountAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).LongCountAsync( cancellation );
    public Task<bool> ExistsAsync( CancellationToken cancellation = default ) => new LiteDbResult<T>( query, queue ).ExistsAsync( cancellation );
}

public interface ILiteDbQueryable<T> : ILiteDbResult<T>
{
    public ILiteDbQueryable<T> Include( BsonExpression path );
    public ILiteDbQueryable<T> Include( List<BsonExpression> paths );
    public ILiteDbQueryable<T> Include<TKey>( Expression<Func<T, TKey>> path );

    public ILiteDbQueryable<T> Where( BsonExpression predicate );
    public ILiteDbQueryable<T> Where( string predicate, BsonDocument parameters );
    public ILiteDbQueryable<T> Where( string predicate, params BsonValue[] args );
    public ILiteDbQueryable<T> Where( Expression<Func<T, bool>> predicate );

    public ILiteDbQueryable<T> OrderBy( BsonExpression selector, int order = 1 );
    public ILiteDbQueryable<T> OrderBy<TKey>( Expression<Func<T, TKey>> selector, int order = 1 );
    public ILiteDbQueryable<T> OrderByDescending( BsonExpression selector );
    public ILiteDbQueryable<T> OrderByDescending<TKey>( Expression<Func<T, TKey>> selector );

    public ILiteDbQueryable<T> GroupBy( BsonExpression selector );
    public ILiteDbQueryable<T> Having( BsonExpression predicate );

    public ILiteDbResult<BsonDocument> Select( BsonExpression selector );
    public ILiteDbResult<TKey> Select<TKey>( Expression<Func<T, TKey>> selector );
}