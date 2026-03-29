namespace LiteDB;

public abstract class LiteDbOptions
{
    public string ConnectionString { get; set; }
}

public sealed class LiteDbOptions<TContext> : LiteDbOptions
    where TContext : LiteDbContext;

