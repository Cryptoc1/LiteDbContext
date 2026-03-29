using LiteDB;

namespace LiteDbContext.Tests;

public sealed class UpsertTests
{
    [Fact]
    public async Task Upsert_Adds( )
    {
        var options = new LiteDbOptions<TestDbContext>
        {
            ConnectionString = $"{Directory.GetCurrentDirectory()}/tests_{Guid.NewGuid()}.db"
        };

        await using( var context = new TestDbContext( options ) )
        {
            var added = await context.Tests.UpsertAsync( new TestEntity
            {
                Key = "Test1",
                Value = "Hello, World!"
            } );

            Assert.True( added );
        }

        if( File.Exists( options.ConnectionString ) )
        {
            File.Delete( options.ConnectionString );
        }
    }
}