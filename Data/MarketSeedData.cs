using BlazorBday.Models;

namespace BlazorBday.Data
{
    /// <summary>
    /// This class is used to seed the database with initial data.
    /// </summary>
    public static class MarketSeedData
    {
        public static void Initialize(MarketShopDbContext context)
        {
            // Ensure the database is created (if not using migrations)
            context.Database.EnsureCreated();

            if (!context.Categories.Any())
            {
                // Category seed data 
                var categories = new List<Category>
                {
                    new Category { Id = 1, Name = "Birthday Card" },
                    new Category { Id = 2, Name = "Book" },
                    new Category { Id = 3, Name = "Gift" },
                    new Category { Id = 4, Name = "Sweet Treat" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            if (!context.Cards.Any())
            {
                // Card seed data
                var cards = new List<Card>
                {
                    new Card
                    {
                        Id = 1,
                        Name = "Happy Birthday - Confetti",
                        Description = "A colorful confetti birthday card with gold foil accents.",
                        Image = "/images/cards/confetti.svg",
                        Points = 3,
                        Inventory = 120,
                        MinAge = 3,
                        MaxAge = 18,
                        CategoryId = 1,
                        Notes = "Popular choice for kids and adults."
                    },
                    new Card
                    {
                        Id = 2,
                        Name = "Handmade Floral",
                        Description = "Handmade card with pressed flowers. Eco-friendly materials.",
                        Image = "/images/cards/floral.svg",
                        Points = 3,
                        Inventory = 40,
                        MinAge = 12,
                        MaxAge = 18,
                        CategoryId = 1,
                        Notes = "Limited stock; handle with care."
                    },
                    new Card
                    {
                        Id = 3,
                        Name = "Kids' Animal Parade",
                        Description = "Bright and playful animal parade card for young children.",
                        Image = "/images/cards/animal_parade.svg",
                        Points = 3,
                        Inventory = 200,
                        MinAge = 0,
                        MaxAge = 10,
                        CategoryId = 1,
                        Notes = "Includes stickers inside."
                    },
                    new Card
                    {
                        Id = 4,
                        Name = "Elegant Script Birthday",
                        Description = "Minimalist card with elegant script and embossed finish.",
                        Image = "/images/cards/elegant_script.svg",
                        Points = 3,
                        Inventory = 75,
                        MinAge = 18,
                        MaxAge = 99,
                        CategoryId = 1,
                        Notes = "Great for formal occasions."
                    },
                    new Card
                    {
                        Id = 5,
                        Name = "Pop-Up Surprise",
                        Description = "Interactive pop-up card with 3D elements and a pull-tab surprise.",
                        Image = "/images/cards/popup_surprise.svg",
                        Points = 3,
                        Inventory = 30,
                        MinAge = 6,
                        MaxAge = 99,
                        CategoryId = 1,
                        Notes = "Fragile – keep flat."
                    },
                    new Card
                    {
                        Id = 6,
                        Name = "Thank You Watercolor",
                        Description = "Soft watercolor thank-you card with hand-painted look and subtle gold accents.",
                        Image = "/images/cards/thank_you_watercolor.svg",
                        Points = 3,
                        Inventory = 85,
                        MinAge = 0,
                        MaxAge = 99,
                        CategoryId = 1,
                        Notes = "Pairs well with ribbon or small gifts."
                    }
                };
                context.Cards.AddRange(cards);
                context.SaveChanges();
            }

            if (!context.Books.Any())
            {
                // Book seed data (example)
                var books = new List<Book>
                {
                    new Book {
                        Id = 1,
                        Name = "The Birthday Surprise",
                        Description = "A sweet story for little ones.",
                        Image = "/images/books/book1.png",
                        Points = 5,
                        Inventory = 40,
                        MinAge = 0,
                        MaxAge = 6,
                        CategoryId = 2,
                        Author = "A. Author"
                    },
                    new Book {
                        Id = 2,
                        Name = "Big Party",
                        Description = "Party planning for kids.",
                        Image = "/images/books/book2.png",
                        Points = 7,
                        Inventory = 20,
                        MinAge = 4,
                        MaxAge = 10,
                        CategoryId = 2,
                        Author = "B. Writer"
                    }
                };
                context.AddRange(books);
                context.SaveChanges();
            }
            
            if (!context.SweetTreats.Any())
            {
                // SweetTreat seed data (example)
                var sweetTreats = new List<SweetTreat>
                {
                    new SweetTreat {
                        Id = 1,
                        Name = "Chocolate Fudge Cake",
                        Description = "Rich and creamy chocolate fudge cake.",
                        Image = "/images/treats/cake-slice.png",
                        Points = 16,
                        Inventory = 50,
                        MinAge = 0,
                        MaxAge = 99,
                        CategoryId = 4,
                    },
                    new SweetTreat {
                        Id = 2,
                        Name = "Lollipops",
                        Description = "Colorful assorted lollipops.",
                        Image = "/images/treats/lollipop.png",
                        Points = 2,
                        Inventory = 100,
                        MinAge = 3,
                        MaxAge = 99,
                        CategoryId = 4,
                        Lettering = "Happy BDay Jeff!",
                        LetteringColor = "Red"
                    },
                    new SweetTreat {
                        Id = 3,
                        Name = "Cupcakes",
                        Description = "Vanilla cupcakes with sprinkles.",
                        Image = "/images/treats/cupcake.png",
                        Points = 4,
                        Inventory = 80,
                        MinAge = 0,
                        MaxAge = 99,
                        CategoryId = 4,
                    },
                    new SweetTreat {
                        Id = 4,
                        Name = "Pumpkin Pie",
                        Description = "Assorted fruit-flavored gummy bears.",
                        Image = "/images/treats/pie.png",
                        Points = 3,
                        Inventory = 150,
                        MinAge = 3,
                        MaxAge = 99,
                        CategoryId = 4,
                        Lettering = "Stay Cool Brother!",
                    }
                };
                context.AddRange(sweetTreats);
                context.SaveChanges();
            }

        }
    }
}