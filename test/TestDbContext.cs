using LiteDB;

namespace LiteDbContext.Tests;

public sealed class TestDbContext : LiteDB.LiteDbContext
{
    public LiteDbSet<TestEntity> Tests { get; }

    public TestDbContext( LiteDbOptions options ) : base( options )
    {
        Tests = DbSet<TestEntity>();
    }
}

public sealed record TestEntity
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}