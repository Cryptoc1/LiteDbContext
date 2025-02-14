namespace LiteDB;

public class LiteDbOptions
{
    public string ConnectionString { get; set; }

    public BsonMapper Mapper { get; set; } = new BsonMapper()
        .ConfigureDates()
        .ConfigureJson()
        .ConfigureTime()
        .ConfigureUris();
}

public class LiteDbOptions<TContext> : LiteDbOptions
    where TContext : LiteDbContext;

