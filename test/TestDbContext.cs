using LiteDB;

namespace LiteDbContext.Tests;

public sealed class TestDbContext( LiteDbOptions<TestDbContext> options ) : LiteDB.LiteDbContext( options )
{
    public LiteDbSet<TestEntity> Tests => DbSet<TestEntity>();

    protected override void OnCreatingMapper( BsonMapper mapper )
    {
        base.OnCreatingMapper( mapper );

        LiteDBPragmas.I_AM_AWARE_MY_DATABASE_BREAKS_WHEN_I_USE_THIS();
    }
}

public sealed record TestEntity
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}