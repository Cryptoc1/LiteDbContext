# LiteDbContext

![Language](https://img.shields.io/github/languages/top/cryptoc1/LiteDbContext)
[![Dependencies](https://img.shields.io/librariesio/github/cryptoc1/LiteDbContext)](https://libraries.io/nuget/LiteDbContext)
[![Checks](https://img.shields.io/github/checks-status/cryptoc1/LiteDbContext/develop)](https://github.com/Cryptoc1/LiteDbContext/actions/workflows/default.yml)
[![Coverage](https://img.shields.io/codecov/c/github/cryptoc1/LiteDbContext)](https://app.codecov.io/gh/Cryptoc1/LiteDbContext/)
[![Version](https://img.shields.io/nuget/vpre/LiteDbContext)](https://www.nuget.org/packages/LiteDbContext)

A light-weight async wrapper around [LiteDB](https://github.com/litedb-org/LiteDB).

### Key Features:
- Modern C#
- Supports cancellations

## Usage

1. Define a `LiteDbContext`:
```csharp
using LiteDB;

// ...

public sealed class ExampleDbContext(LiteDbOptions options) : LiteDbContext(options)
{
    // NOTE: if a `name` is not provided to `DbSet<T>(string? name)`, `[CallerMemberName]` will provide the `PropertyInfo.Name` for you, e.g. `Examples`
    public LiteDbSet<ExampleEntity> Examples => DbSet<ExampleEntity>();
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
