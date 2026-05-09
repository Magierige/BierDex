using BierDex.Data;
using BierDex.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BierDex.Controllers
{
    [ApiController]
    [Route("api/beer")]
    public class BeerController : ControllerBase
    {
        private readonly BierdexDBContext _context;

        // The DbContext is injected via the constructor
        public BeerController(BierdexDBContext context)
        {
            _context = context;
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
            // 1. Get the Unique Identifier (ID) from the current user's claims
            // This assumes the ID is stored in the 'NameIdentifier' claim
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
    }
}
