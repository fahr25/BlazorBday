using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlazorBday.Data;
using BlazorBday.Models;

namespace BlazorBday.Controllers
{
    /// <summary>
    /// This class creates a controller that allows us to query the database for shop items 
    /// and returns them as JSON at the (http://localhost:5000/api/marketshop) URL.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MarketShopController : ControllerBase
    {
        private readonly MarketShopDbContext _db;

        public MarketShopController(MarketShopDbContext dbContext)
        {
            _db = dbContext;
        }

        [HttpGet("cards")]
        public async Task<ActionResult<List<Card>>> GetAllCards()
        {
            var cards = await _db.Cards.ToListAsync();
            return Ok(cards);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<Category>>> GetAllCategories()
        {
            var categories = await _db.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("books")]
        public async Task<ActionResult<List<Book>>> GetAllBooks()
        {
            var books = await _db.Books.ToListAsync();
            return Ok(books);
        }

        [HttpGet("treats")]
        public async Task<ActionResult<List<SweetTreat>>> GetAllSweetTreats()
        {
            var treats = await _db.SweetTreats.ToListAsync();
            return Ok(treats);
        }

    }
}