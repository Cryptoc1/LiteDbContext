namespace LiteDB;

public class LiteDbOptions
{
    public string ConnectionString { get; set; }
    public BsonMapper Mapper { get; } = new();
}

public class LiteDbOptions<TContext> : LiteDbOptions
    where TContext : LiteDbContext;