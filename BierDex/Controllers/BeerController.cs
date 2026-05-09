using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BierDex.Services;

namespace BierDex.Controllers
{
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
        public async Task<ActionResult<Beer>> CreateBeer([FromBody] Beer beer)
        {
            // Koppel het bier automatisch aan de ingelogde gebruiker
            beer.userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.Beers.Add(beer);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAllBeers), new { id = beer.Id }, beer);
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
