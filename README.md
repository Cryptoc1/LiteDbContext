# LiteDbContext

A light-weight async wrapper around [LiteDB](https://github.com/litedb-org/LiteDB).

### Key Features:
- Modern C#
- Supports cancellations

## Usage

1. Define a `LiteDbContext`:
```csharp
using LiteDB;

// ...

public sealed class ExampleDbContext : LiteDbContext
{
    public LiteDbSet<ExampleEntity> Examples { get; }

    public ExampleDbContext(LiteDbOptions options) : base(options)
    {
        Examples = DbSet<ExampleEntity>()
            .OnCreatingCollection(OnCreatingExamples)

            /*
            * Override the collection name.
            * default: `PropertyInfo.Name`, provided to `DbSet<T>()` via `[CallerMemberName]`
            */
            .WithName("examples");
    }

    private static void OnCreatingExamples(ILiteCollection<ExampleEntity> collection)
    {
        collection.EnsureIndex(example => example.Value);
    }
}

public sealed record class ExampleEntity
{
    public required string Key { get; init; }
    public string Value { get; init; }
}
```

2. Add your `LiteDbContext` as a service:
```csharp

using LiteDB.Extensions.DependencyInjection;

// ...

IServiceCollection services;

 // ...

services.AddLiteDbContext<ExampleDbContext>(options =>
{
    options.ConnectionString = "...";

    options.Mapper.ConfigureExamples();
});

// ...

internal static class ExampleEntityConfiguration
{
    public static BsonMapper ConfigureExamples(this BsonMapper mapper)
    {
        mapper.Entity<ExampleEntity>()
            .Id(example => example.Key);

        return mapper;
    }
}
```

3. Use you `LiteDbContext`:
```csharp
public sealed class ExampleController
{
    [HttpGet]
    public async Task<ActionResult<ExampleEntity[]>> Index(
        [FromServices] ExampleDbContext context,
        [FromQuery, Required] string query)
    {
        var examples = await context.Examples.Query()
            .Where(example => example.Value.StartsWith( query ))
            .ToArrayAsync(HttpContext.RequestAborted);

        return Ok(examples);
    }
}
```

## _How'd He Do It?_

Work against the database is synchronized via a queue, backed by an unbounded `Channel`.

See [`DbWorkQueue`](https://github.com/Cryptoc1/LiteDbContext/blob/develop/src/DbWorkQueue.cs).
