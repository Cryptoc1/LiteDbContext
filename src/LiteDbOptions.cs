namespace LiteDB;

public class LiteDbOptions
{
    public string ConnectionString { get; set; }

    public BsonMapper Mapper { get; set; } = new BsonMapper()
        .ConfigureDates()
        .ConfigureJson()
        .ConfigureTime()
        .ConfigureUris();

    public int UserVersion { get; set; }
}

public class LiteDbOptions<TContext> : LiteDbOptions
    where TContext : LiteDbContext;

