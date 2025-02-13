using System.Linq.Expressions;

namespace LiteDB;

public sealed class LiteDbSet<T> : ILiteDbSet<T>
{
    private readonly ILiteCollection<T> collection;
    private readonly DbWorkQueue queue;

    public string Name => collection.Name;
    public BsonAutoId AutoId => collection.AutoId;
    public EntityMapper EntityMapper => collection.EntityMapper;

    internal LiteDbSet( ILiteCollection<T> collection, DbWorkQueue queue )
    {
        this.collection = collection;
        this.queue = queue;
    }

    public ILiteDbSet<T> Include<TKey>( Expression<Func<T, TKey>> selector ) => new LiteDbSet<T>(
        collection.Include( selector ),
        queue );

    public ILiteDbSet<T> Include( BsonExpression selector ) => new LiteDbSet<T>(
        collection.Include( selector ),
        queue );

    public Task<bool> UpsertAsync( T entity, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Upsert( entity ),
        cancellation );

    public Task<int> UpsertAsync( IEnumerable<T> entities, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Upsert( entities ),
        cancellation );

    public Task<bool> UpsertAsync( BsonValue id, T entity, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Upsert( id, entity ),
        cancellation );

    public Task<bool> UpdateAsync( T entity, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Update( entity ),
        cancellation );

    public Task<bool> UpdateAsync( BsonValue id, T entity, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Update( id, entity ),
        cancellation );

    public Task<int> UpdateAsync( IEnumerable<T> entities, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Update( entities ),
        cancellation );

    public Task<int> UpdateManyAsync( BsonExpression transform, BsonExpression predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.UpdateMany( transform, predicate ),
        cancellation );

    public Task<int> UpdateManyAsync( Expression<Func<T, T>> extend, Expression<Func<T, bool>> predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.UpdateMany( extend, predicate ),
        cancellation );

    public Task<BsonValue> InsertAsync( T entity, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Insert( entity ),
        cancellation );

    public Task InsertAsync( BsonValue id, T entity, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Insert( id, entity ),
        cancellation );

    public Task<int> InsertAsync( IEnumerable<T> entities, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Insert( entities ),
        cancellation );

    public Task<int> InsertBulkAsync( IEnumerable<T> entities, int batchSize = 5000, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.InsertBulk( entities, batchSize ),
        cancellation );

    public Task<bool> EnsureIndexAsync( string name, BsonExpression expression, bool unique = false, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.EnsureIndex( name, expression, unique ),
        cancellation );

    public Task<bool> EnsureIndexAsync( BsonExpression expression, bool unique = false, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.EnsureIndex( expression, unique ),
        cancellation );

    public Task<bool> EnsureIndexAsync<TKey>( Expression<Func<T, TKey>> selector, bool unique = false, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.EnsureIndex( selector, unique ),
        cancellation );

    public Task<bool> EnsureIndexAsync<TKey>( string name, Expression<Func<T, TKey>> selector, bool unique = false, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.EnsureIndex( name, selector, unique ),
        cancellation );

    public Task<bool> DropIndexAsync( string name, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.DropIndex( name ),
        cancellation );

    public ILiteDbQueryable<T> Query( ) => new LiteDbQueryable<T>(
        collection.Query(),
        queue );

    public IAsyncEnumerable<T> FindAsync( BsonExpression predicate, int skip = 0, int limit = int.MaxValue, CancellationToken cancellation = default ) => queue.EnumerateAsync(
        ( ) => collection.Find( predicate, skip, limit ),
        cancellation );

    public IAsyncEnumerable<T> FindAsync( Query query, int skip = 0, int limit = int.MaxValue, CancellationToken cancellation = default ) => queue.EnumerateAsync(
        ( ) => collection.Find( query, skip, limit ),
        cancellation );

    public IAsyncEnumerable<T> FindAsync( Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue, CancellationToken cancellation = default ) => queue.EnumerateAsync(
        ( ) => collection.Find( predicate, skip, limit ),
        cancellation );

    public Task<T?> FindByIdAsync( BsonValue id, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.FindById( id ),
        cancellation )!;

    public Task<T?> FindOneAsync( BsonExpression predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.FindOne( predicate ),
        cancellation )!;

    public Task<T?> FindOneAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.FindOne( predicate, parameters ),
        cancellation )!;

    public Task<T?> FindOneAsync( BsonExpression predicate, BsonValue[] keys, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.FindOne( predicate, keys ),
        cancellation )!;

    public Task<T?> FindOneAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.FindOne( predicate ),
        cancellation )!;

    public Task<T?> FindOneAsync( Query query, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.FindOne( query ),
        cancellation )!;

    public IAsyncEnumerable<T> FindAllAsync( CancellationToken cancellation = default ) => queue.EnumerateAsync(
        ( ) => collection.FindAll(),
        cancellation );

    public Task<bool> DeleteAsync( BsonValue id, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Delete( id ),
        cancellation );

    public Task<int> DeleteAllAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.DeleteAll(),
        cancellation );

    public Task<int> DeleteManyAsync( BsonExpression predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.DeleteMany( predicate ),
        cancellation );

    public Task<int> DeleteManyAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.DeleteMany( predicate, parameters ),
        cancellation );

    public Task<int> DeleteManyAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.DeleteMany( predicate, keys ),
        cancellation );

    public Task<int> DeleteManyAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.DeleteMany( predicate ),
        cancellation );

    public Task<int> CountAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Count(),
        cancellation );

    public Task<int> CountAsync( BsonExpression predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Count( predicate ),
        cancellation );

    public Task<int> CountAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Count( predicate, parameters ),
        cancellation );

    public Task<int> CountAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Count( predicate, keys ),
        cancellation );

    public Task<int> CountAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Count( predicate ),
        cancellation );

    public Task<int> CountAsync( Query query, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Count( query ),
        cancellation );

    public Task<long> LongCountAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.LongCount(),
        cancellation );

    public Task<long> LongCountAsync( BsonExpression predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.LongCount(),
        cancellation );

    public Task<long> LongCountAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.LongCount( predicate, parameters ),
        cancellation );

    public Task<long> LongCountAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.LongCount( predicate, keys ),
        cancellation );

    public Task<long> LongCountAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.LongCount( predicate ),
        cancellation );

    public Task<long> LongCountAsync( Query query, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.LongCount( query ),
        cancellation );

    public Task<bool> ExistsAsync( BsonExpression predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Exists( predicate ),
        cancellation );

    public Task<bool> ExistsAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Exists( predicate, parameters ),
        cancellation );

    public Task<bool> ExistsAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Exists( predicate, keys ),
        cancellation );

    public Task<bool> ExistsAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Exists( predicate ),
        cancellation );

    public Task<bool> ExistsAsync( Query query, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Exists( query ),
        cancellation );

    public Task<BsonValue> MaxAsync( BsonExpression selector, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Max( selector ),
        cancellation );

    public Task<BsonValue> MaxAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Max(),
        cancellation );

    public Task<TResult> MaxAsync<TResult>( Expression<Func<T, TResult>> selector, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Max( selector ),
        cancellation );

    public Task<BsonValue> MinAsync( BsonExpression selector, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Min( selector ),
        cancellation );

    public Task<BsonValue> MinAsync( CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Min(),
        cancellation );

    public Task<TResult> MinAsync<TResult>( Expression<Func<T, TResult>> selector, CancellationToken cancellation = default ) => queue.InvokeAsync(
        ( ) => collection.Min( selector ),
        cancellation );
}

public interface ILiteDbSet<T>
{
    public string Name { get; }
    public BsonAutoId AutoId { get; }
    public EntityMapper EntityMapper { get; }

    public ILiteDbSet<T> Include<TKey>( Expression<Func<T, TKey>> selector );
    public ILiteDbSet<T> Include( BsonExpression selector );

    public Task<bool> UpsertAsync( T entity, CancellationToken cancellation = default );
    public Task<int> UpsertAsync( IEnumerable<T> entities, CancellationToken cancellation = default );
    public Task<bool> UpsertAsync( BsonValue id, T entity, CancellationToken cancellation = default );

    public Task<bool> UpdateAsync( T entity, CancellationToken cancellation = default );
    public Task<bool> UpdateAsync( BsonValue id, T entity, CancellationToken cancellation = default );
    public Task<int> UpdateAsync( IEnumerable<T> entities, CancellationToken cancellation = default );
    public Task<int> UpdateManyAsync( BsonExpression transform, BsonExpression predicate, CancellationToken cancellation = default );
    public Task<int> UpdateManyAsync( Expression<Func<T, T>> extend, Expression<Func<T, bool>> predicate, CancellationToken cancellation = default );

    public Task<BsonValue> InsertAsync( T entity, CancellationToken cancellation = default );
    public Task InsertAsync( BsonValue id, T entity, CancellationToken cancellation = default );
    public Task<int> InsertAsync( IEnumerable<T> entities, CancellationToken cancellation = default );
    public Task<int> InsertBulkAsync( IEnumerable<T> entities, int batchSize = 5000, CancellationToken cancellation = default );

    public Task<bool> EnsureIndexAsync( string name, BsonExpression expression, bool unique = false, CancellationToken cancellation = default );
    public Task<bool> EnsureIndexAsync( BsonExpression expression, bool unique = false, CancellationToken cancellation = default );
    public Task<bool> EnsureIndexAsync<TKey>( Expression<Func<T, TKey>> selector, bool unique = false, CancellationToken cancellation = default );
    public Task<bool> EnsureIndexAsync<TKey>( string name, Expression<Func<T, TKey>> selector, bool unique = false, CancellationToken cancellation = default );

    public Task<bool> DropIndexAsync( string name, CancellationToken cancellation = default );

    public ILiteDbQueryable<T> Query( );

    public IAsyncEnumerable<T> FindAsync( BsonExpression predicate, int skip = 0, int limit = int.MaxValue, CancellationToken cancellation = default );
    public IAsyncEnumerable<T> FindAsync( Query query, int skip = 0, int limit = int.MaxValue, CancellationToken cancellation = default );
    public IAsyncEnumerable<T> FindAsync( Expression<Func<T, bool>> predicate, int skip = 0, int limit = int.MaxValue, CancellationToken cancellation = default );
    public Task<T?> FindByIdAsync( BsonValue id, CancellationToken cancellation = default );
    public Task<T?> FindOneAsync( BsonExpression predicate, CancellationToken cancellation = default );
    public Task<T?> FindOneAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default );
    public Task<T?> FindOneAsync( BsonExpression predicate, BsonValue[] keys, CancellationToken cancellation = default );
    public Task<T?> FindOneAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default );
    public Task<T?> FindOneAsync( Query query, CancellationToken cancellation = default );
    public IAsyncEnumerable<T> FindAllAsync( CancellationToken cancellation = default );

    public Task<bool> DeleteAsync( BsonValue id, CancellationToken cancellation = default );
    public Task<int> DeleteAllAsync( CancellationToken cancellation = default );
    public Task<int> DeleteManyAsync( BsonExpression predicate, CancellationToken cancellation = default );
    public Task<int> DeleteManyAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default );
    public Task<int> DeleteManyAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default );
    public Task<int> DeleteManyAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default );

    public Task<int> CountAsync( CancellationToken cancellation = default );
    public Task<int> CountAsync( BsonExpression predicate, CancellationToken cancellation = default );
    public Task<int> CountAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default );
    public Task<int> CountAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default );
    public Task<int> CountAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default );
    public Task<int> CountAsync( Query query, CancellationToken cancellation = default );

    public Task<long> LongCountAsync( CancellationToken cancellation = default );
    public Task<long> LongCountAsync( BsonExpression predicate, CancellationToken cancellation = default );
    public Task<long> LongCountAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default );
    public Task<long> LongCountAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default );
    public Task<long> LongCountAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default );
    public Task<long> LongCountAsync( Query query, CancellationToken cancellation = default );

    public Task<bool> ExistsAsync( BsonExpression predicate, CancellationToken cancellation = default );
    public Task<bool> ExistsAsync( string predicate, BsonDocument parameters, CancellationToken cancellation = default );
    public Task<bool> ExistsAsync( string predicate, BsonValue[] keys, CancellationToken cancellation = default );
    public Task<bool> ExistsAsync( Expression<Func<T, bool>> predicate, CancellationToken cancellation = default );
    public Task<bool> ExistsAsync( Query query, CancellationToken cancellation = default );

    public Task<BsonValue> MaxAsync( BsonExpression selector, CancellationToken cancellation = default );
    public Task<BsonValue> MaxAsync( CancellationToken cancellation = default );
    public Task<TResult> MaxAsync<TResult>( Expression<Func<T, TResult>> selector, CancellationToken cancellation = default );

    public Task<BsonValue> MinAsync( BsonExpression selector, CancellationToken cancellation = default );
    public Task<BsonValue> MinAsync( CancellationToken cancellation = default );
    public Task<TResult> MinAsync<TResult>( Expression<Func<T, TResult>> selector, CancellationToken cancellation = default );
}