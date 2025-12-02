using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BlazorBday.Data;

namespace BlazorBday.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration testing.
/// Replaces the real database with an in-memory database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MarketShopDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using in-memory database for testing
            services.AddDbContext<MarketShopDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Create a scope to obtain a reference to the database context
            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<MarketShopDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed test data if needed
                SeedTestData(db);
            }
        });
    }

    private void SeedTestData(MarketShopDbContext context)
    {
        // Add test data for integration tests
        // Example: agencies, products, categories, etc.
    }
}
