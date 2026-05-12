using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using BierDex.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BierDex.Controllers
{
    public record BeerCreateRequest(int barcode, string name, string type, double abv, string imagePath);

    [ApiController]
    [Route("api/beer")]
    public class BeerController : ControllerBase
    {
        private readonly BierdexDBContext _context;
        private readonly BeerService _beerService;

        // The DbContext is injected via the constructor
        public BeerController(BierdexDBContext context, BeerService beerService)
        {
            _context = context;
            _beerService = beerService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Beer>>> GetAllBeers()
        {
            // Use ToListAsync to keep the API responsive and non-blocking
            var beers = await _context.Beers.ToListAsync();

            return Ok(beers);
        }

        [HttpGet("my-beers")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Beer>>> GetUserBeers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            // 2. Filter the database query by that UserId
            var userBeers = await _context.Beers
                .Where(b => b.userId == userId)
                .ToListAsync();

            return Ok(userBeers);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Beer>> CreateBeer([FromBody] BeerCreateRequest beer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isSupplier = User.IsInRole("Supplier");

            var newBeer = new Beer
            {
                name = beer.name,
                type = beer.type,
                abv = beer.abv.ToString() + "%",
                imagePath = beer.imagePath,
                approved = false, // New beers are not approved by default
                userId = userId,
                barcode = beer.barcode
            };

            var result = await _beerService.CreateBeerAsync(newBeer, userId, isAdmin, isSupplier);

            if (!result.Success) return Unauthorized(result.Message);
            return CreatedAtAction(nameof(GetAllBeers), new { id = result.Beer?.Id }, result.Beer);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBeer(int id, [FromBody] Beer beer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin"); // Checkt of de JWT de Admin role heeft

            var result = await _beerService.UpdateBeerAsync(id, beer, userId, isAdmin);

            if (!result.Success) return Unauthorized(result.Message);
            return Ok(result.Beer);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBeer(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var result = await _beerService.DeleteBeerAsync(id, userId, isAdmin);

            if (!result.Success) return Unauthorized(result.Message);
            return NoContent();
        }
    }
}
