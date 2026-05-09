using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BierDex.Data;
using BierDex.Models;

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
    }
}
