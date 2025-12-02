using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;

namespace BlazorBday.Tests.Helpers;

public static class TestDbContextFactory
{
    /// <summary>
    /// Creates an in-memory MarketShopDbContext for testing purposes.
    /// Each test gets a unique database to ensure isolation.
    /// </summary>
    public static MarketShopDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<MarketShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new MarketShopDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates an in-memory context with a specific database name.
    /// Useful when you need to share data between multiple context instances in a test.
    /// </summary>
    public static MarketShopDbContext CreateInMemoryContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<MarketShopDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        var context = new MarketShopDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
