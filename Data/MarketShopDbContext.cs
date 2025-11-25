using Microsoft.EntityFrameworkCore;

namespace BlazorBday.Data
{
    /// <summary>
    /// This class creates a database context we can use to register a database service. 
    /// The context also allows us to have a controller that accesses the database.
    /// </summary>
    public class MarketShopDbContext : DbContext
    {
        public MarketShopDbContext(DbContextOptions<MarketShopDbContext> options): base(options)
        {
        }

        public DbSet<Product> Cards { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Gift> Gifts { get; set; }
    }
}