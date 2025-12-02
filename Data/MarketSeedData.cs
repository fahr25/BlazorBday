using BlazorBday.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorBday.Data;



/// <summary>
/// This class is used to seed the database with initial data.
/// </summary>
public class MarketSeedData
{

    private readonly MarketShopDbContext _db;

    public MarketSeedData(MarketShopDbContext db)
    {
        _db = db;
    }

    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Check if any users exist
        if (userManager.Users.Any())
        {
            return; // Admin already exists
        }

        // Create default admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@celebrateme.com",
            Email = "admin@celebrateme.com",
            EmailConfirmed = true
        };

        // Default password - should be changed after first login
        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    
    public async Task SeedAsync()
    {
        // Ensure required categories exist
        var cardsCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Cards");
        if (cardsCategory == null)
        {
            cardsCategory = new Category
            {
                Name = "Cards",
                Description = "Birthday cards",
                DisplayOrder = 10,
                IsActive = true
            };
            _db.Categories.Add(cardsCategory);
        }

        var booksCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Books");
        if (booksCategory == null)
        {
            booksCategory = new Category
            {
                Name = "Books",
                Description = "Books for all ages",
                DisplayOrder = 20,
                IsActive = true
            };
            _db.Categories.Add(booksCategory);
        }

        var treatsCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Treats");
        if (treatsCategory == null)
        {
            treatsCategory = new Category
            {
                Name = "Treats",
                Description = "Sweet treats and snacks",
                DisplayOrder = 30,
                IsActive = true
            };
            _db.Categories.Add(treatsCategory);
        }

        var giftsCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Gifts");
        if (giftsCategory == null)
        {
            giftsCategory = new Category
            {
                Name = "Gifts",
                Description = "Gift items",
                DisplayOrder = 40,
                IsActive = true
            };
            _db.Categories.Add(giftsCategory);
        }

        await _db.SaveChangesAsync();

        // Refresh references in case they were just created
        cardsCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Cards");
        booksCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Books");
        treatsCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Treats");
        giftsCategory = await _db.Categories.FirstOrDefaultAsync(c => c.Name == "Gifts");

        // Check if products already exist
        if (await _db.Products.AnyAsync())
        {
            Console.WriteLine("Products already exist. Skipping seed.");
            return;
        }

        Console.WriteLine("Seeding products based on uploaded images...");

        // Create subcategories for Gifts
        var toysSubcat = new Subcategory { Name = "Toys", Description = "Fun toys for kids", CategoryId = giftsCategory.Id, IsActive = true };
        var techSubcat = new Subcategory { Name = "Electronics", Description = "Tech gadgets and accessories", CategoryId = giftsCategory.Id, IsActive = true };
        var giftCardsSubcat = new Subcategory { Name = "Gift Cards", Description = "Store gift cards", CategoryId = giftsCategory.Id, IsActive = true };
        var sportsSubcat = new Subcategory { Name = "Sports", Description = "Sports equipment and toys", CategoryId = giftsCategory.Id, IsActive = true };

        _db.Subcategories.AddRange(toysSubcat, techSubcat, giftCardsSubcat, sportsSubcat);
        await _db.SaveChangesAsync();

        // Seed Cards - matching uploaded images (5-8 points each)
        var cards = new List<Product>
        {
            new Product
            {
                Name = "Thank You Watercolor Card",
                Description = "Beautiful watercolor birthday wishes",
                Points = 5,
                Inventory = 50,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/thank_you_watercolor.svg"
            },
            new Product
            {
                Name = "Pop-Up Surprise Card",
                Description = "Fun pop-up birthday surprise",
                Points = 7,
                Inventory = 40,
                MinAge = 3,
                MaxAge = 18,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/popup_surprise.svg"
            },
            new Product
            {
                Name = "Confetti Birthday Card",
                Description = "Colorful confetti celebration design",
                Points = 5,
                Inventory = 50,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/confetti.svg"
            },
            new Product
            {
                Name = "Animal Parade Card",
                Description = "Cute animals celebrating your birthday",
                Points = 6,
                Inventory = 45,
                MinAge = 0,
                MaxAge = 10,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/animal_parade.svg"
            },
            new Product
            {
                Name = "Elegant Script Card",
                Description = "Classic elegant birthday wishes",
                Points = 5,
                Inventory = 50,
                MinAge = 10,
                MaxAge = 18,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/elegant_script.svg"
            },
            new Product
            {
                Name = "Floral Birthday Card",
                Description = "Beautiful floral design",
                Points = 5,
                Inventory = 50,
                MinAge = 5,
                MaxAge = 18,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/floral.svg"
            },
            new Product
            {
                Name = "Special Birthday Card",
                Description = "Premium birthday card design",
                Points = 6,
                Inventory = 45,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = cardsCategory.Id,
                Image = "/images/cards/card-4.jpg"
            }
        };

        // Seed Books - matching uploaded images (12-18 points each)
        var books = new List<Product>
        {
            new Product
            {
                Name = "Twenty-Four Seconds",
                Description = "Inspiring basketball story",
                Points = 15,
                Inventory = 25,
                MinAge = 8,
                MaxAge = 14,
                CategoryId = booksCategory.Id,
                Image = "/images/books/Twenty-Four-Seconds.jpg"
            },
            new Product
            {
                Name = "Adventure Story Book",
                Description = "Exciting adventure for young readers",
                Points = 12,
                Inventory = 30,
                MinAge = 6,
                MaxAge = 12,
                CategoryId = booksCategory.Id,
                Image = "/images/books/book2.png"
            },
            new Product
            {
                Name = "Picture Story Book",
                Description = "Beautifully illustrated story",
                Points = 12,
                Inventory = 30,
                MinAge = 3,
                MaxAge = 8,
                CategoryId = booksCategory.Id,
                Image = "/images/books/book1.png"
            },
            // Add more variety books
            new Product
            {
                Name = "The Very Hungry Caterpillar",
                Description = "Classic children's picture book",
                Points = 12,
                Inventory = 30,
                MinAge = 0,
                MaxAge = 5,
                CategoryId = booksCategory.Id
            },
            new Product
            {
                Name = "Charlotte's Web",
                Description = "Heartwarming story of friendship",
                Points = 15,
                Inventory = 25,
                MinAge = 7,
                MaxAge = 12,
                CategoryId = booksCategory.Id
            },
            new Product
            {
                Name = "Harry Potter Book 1",
                Description = "The beginning of a magical journey",
                Points = 18,
                Inventory = 20,
                MinAge = 8,
                MaxAge = 14,
                CategoryId = booksCategory.Id
            },
            new Product
            {
                Name = "Diary of a Wimpy Kid",
                Description = "Hilarious middle school adventures",
                Points = 15,
                Inventory = 30,
                MinAge = 8,
                MaxAge = 13,
                CategoryId = booksCategory.Id
            },
            new Product
            {
                Name = "Wonder",
                Description = "Inspiring story about kindness",
                Points = 16,
                Inventory = 25,
                MinAge = 10,
                MaxAge = 14,
                CategoryId = booksCategory.Id
            }
        };

        // Seed Treats - matching uploaded images (5-12 points each)
        var treats = new List<Product>
        {
            new Product
            {
                Name = "Birthday Pie Slice",
                Description = "Delicious slice of birthday pie",
                Points = 10,
                Inventory = 40,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = treatsCategory.Id,
                Image = "/images/treats/pie.png"
            },
            new Product
            {
                Name = "Rainbow Lollipop",
                Description = "Large colorful swirl lollipop",
                Points = 5,
                Inventory = 60,
                MinAge = 3,
                MaxAge = 18,
                CategoryId = treatsCategory.Id,
                Image = "/images/treats/lollipop.png"
            },
            new Product
            {
                Name = "Birthday Cake Slice",
                Description = "Delicious slice of birthday cake",
                Points = 12,
                Inventory = 35,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = treatsCategory.Id,
                Image = "/images/treats/cake-slice.png"
            },
            new Product
            {
                Name = "Birthday Cupcake",
                Description = "Frosted cupcake with sprinkles",
                Points = 8,
                Inventory = 50,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = treatsCategory.Id,
                Image = "/images/treats/cupcake.png"
            },
            // Add more variety treats
            new Product
            {
                Name = "Chocolate Chip Cookie Pack",
                Description = "Pack of 4 homemade cookies",
                Points = 10,
                Inventory = 50,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = treatsCategory.Id
            },
            new Product
            {
                Name = "Gummy Bear Bag",
                Description = "Colorful gummy bears",
                Points = 6,
                Inventory = 70,
                MinAge = 3,
                MaxAge = 18,
                CategoryId = treatsCategory.Id
            },
            new Product
            {
                Name = "Brownie Bites Box",
                Description = "Box of 6 fudgy brownie bites",
                Points = 10,
                Inventory = 45,
                MinAge = 0,
                MaxAge = 18,
                CategoryId = treatsCategory.Id
            },
            new Product
            {
                Name = "Fruit Snacks Pack",
                Description = "Assorted fruit flavored snacks",
                Points = 6,
                Inventory = 70,
                MinAge = 2,
                MaxAge = 18,
                CategoryId = treatsCategory.Id
            }
        };

        // Seed Gifts - matching uploaded images
        var gifts = new List<Product>
        {
            // Sports - matching image
            new Product
            {
                Name = "Toddler Sports Ball Set",
                Description = "Set of soft balls for toddler sports play",
                Points = 18,
                Inventory = 25,
                MinAge = 1,
                MaxAge = 5,
                CategoryId = giftsCategory.Id,
                SubcategoryId = sportsSubcat.Id,
                Image = "/images/gifts/Balls for Toddler Sports.png"
            },
            // Gift Cards - matching image
            new Product
            {
                Name = "Ulta Gift Card - $25",
                Description = "$25 Ulta Beauty gift card",
                Points = 25,
                Inventory = 30,
                MinAge = 12,
                MaxAge = 18,
                CategoryId = giftsCategory.Id,
                SubcategoryId = giftCardsSubcat.Id,
                Image = "/images/gifts/ulta-giftcard.jpg"
            },
            new Product
            {
                Name = "Ulta Gift Card - $50",
                Description = "$50 Ulta Beauty gift card",
                Points = 50,
                Inventory = 15,
                MinAge = 12,
                MaxAge = 18,
                CategoryId = giftsCategory.Id,
                SubcategoryId = giftCardsSubcat.Id,
                Image = "/images/gifts/ulta-giftcard.jpg"
            },
            // Toys - matching image
            new Product
            {
                Name = "Wooden Yo-Yo",
                Description = "Classic wooden yo-yo toy",
                Points = 10,
                Inventory = 40,
                MinAge = 6,
                MaxAge = 14,
                CategoryId = giftsCategory.Id,
                SubcategoryId = toysSubcat.Id,
                Image = "/images/gifts/yo-yo.jpg"
            },
            // Electronics - matching image
            new Product
            {
                Name = "Wireless Headphones",
                Description = "Bluetooth wireless headphones",
                Points = 45,
                Inventory = 20,
                MinAge = 10,
                MaxAge = 18,
                CategoryId = giftsCategory.Id,
                SubcategoryId = techSubcat.Id,
                Image = "/images/gifts/Wireless_headphones.jpg"
            },

            // Add more gift variety without images
            new Product
            {
                Name = "Building Blocks Set (50pc)",
                Description = "Colorful building blocks",
                Points = 15,
                Inventory = 25,
                MinAge = 3,
                MaxAge = 8,
                CategoryId = giftsCategory.Id,
                SubcategoryId = toysSubcat.Id
            },
            new Product
            {
                Name = "Plush Teddy Bear",
                Description = "Soft and cuddly teddy bear",
                Points = 12,
                Inventory = 30,
                MinAge = 0,
                MaxAge = 10,
                CategoryId = giftsCategory.Id,
                SubcategoryId = toysSubcat.Id
            },
            new Product
            {
                Name = "Hot Wheels 5-Pack",
                Description = "Set of 5 die-cast cars",
                Points = 18,
                Inventory = 35,
                MinAge = 3,
                MaxAge = 12,
                CategoryId = giftsCategory.Id,
                SubcategoryId = toysSubcat.Id
            },
            new Product
            {
                Name = "LEGO Starter Set",
                Description = "LEGO building set with 100 pieces",
                Points = 25,
                Inventory = 20,
                MinAge = 5,
                MaxAge = 14,
                CategoryId = giftsCategory.Id,
                SubcategoryId = toysSubcat.Id
            },
            new Product
            {
                Name = "Soccer Ball (Size 3)",
                Description = "Youth soccer ball",
                Points = 20,
                Inventory = 25,
                MinAge = 5,
                MaxAge = 12,
                CategoryId = giftsCategory.Id,
                SubcategoryId = sportsSubcat.Id
            },
            new Product
            {
                Name = "Basketball (Youth)",
                Description = "Size 5 basketball",
                Points = 22,
                Inventory = 20,
                MinAge = 7,
                MaxAge = 14,
                CategoryId = giftsCategory.Id,
                SubcategoryId = sportsSubcat.Id
            },
            new Product
            {
                Name = "Jump Rope",
                Description = "Colorful jump rope",
                Points = 10,
                Inventory = 40,
                MinAge = 5,
                MaxAge = 14,
                CategoryId = giftsCategory.Id,
                SubcategoryId = sportsSubcat.Id
            },
            new Product
            {
                Name = "Amazon Gift Card - $15",
                Description = "$15 Amazon gift card",
                Points = 15,
                Inventory = 40,
                MinAge = 8,
                MaxAge = 18,
                CategoryId = giftsCategory.Id,
                SubcategoryId = giftCardsSubcat.Id
            },
            new Product
            {
                Name = "Amazon Gift Card - $25",
                Description = "$25 Amazon gift card",
                Points = 25,
                Inventory = 30,
                MinAge = 8,
                MaxAge = 18,
                CategoryId = giftsCategory.Id,
                SubcategoryId = giftCardsSubcat.Id
            },
            new Product
            {
                Name = "Target Gift Card - $20",
                Description = "$20 Target gift card",
                Points = 20,
                Inventory = 35,
                MinAge = 8,
                MaxAge = 18,
                CategoryId = giftsCategory.Id,
                SubcategoryId = giftCardsSubcat.Id
            }
        };

        // Add all products to database
        _db.Products.AddRange(cards);
        _db.Products.AddRange(books);
        _db.Products.AddRange(treats);
        _db.Products.AddRange(gifts);

        await _db.SaveChangesAsync();

        Console.WriteLine($"✓ Seeded {cards.Count} cards (7 with images)");
        Console.WriteLine($"✓ Seeded {books.Count} books (3 with images)");
        Console.WriteLine($"✓ Seeded {treats.Count} treats (4 with images)");
        Console.WriteLine($"✓ Seeded {gifts.Count} gifts (5 with images)");
        Console.WriteLine($"✓ Created 4 gift subcategories");
        Console.WriteLine("Database seeding completed successfully!");
    }

}
