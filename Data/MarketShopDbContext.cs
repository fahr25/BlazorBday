using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BlazorBday.Models;

namespace BlazorBday.Data;

/// <summary>
/// This class creates a database context we can use to register a database service. 
/// The context also allows us to have a controller that accesses the database.
/// </summary>
public class MarketShopDbContext : IdentityDbContext<ApplicationUser>
{
    public MarketShopDbContext(DbContextOptions<MarketShopDbContext> options): base(options)
    {
    }

        public DbSet<Card> Cards { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
    }
}