using LiteDB;

namespace LiteDbContext.Tests;

public sealed class TestDbContext( LiteDbOptions<TestDbContext> options ) : LiteDB.LiteDbContext( options )
{
    public LiteDbSet<TestEntity> Tests => DbSet<TestEntity>();
}

public sealed record TestEntity
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}