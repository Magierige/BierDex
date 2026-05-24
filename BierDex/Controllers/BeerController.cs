using Azure.Core;
using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BierDex.Controllers
{
    public record BeerCreateRequest(
        [Required(ErrorMessage = "Barcode is verplicht")]
        [RegularExpression(@"^\d{8,13}$", ErrorMessage = "Ongeldig formaat. Gebruik een geldige barcode van 8 tot 13 cijfers.")]
        string barcode,

        [Required(ErrorMessage = "Naam is verplicht")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Naam moet tussen 2 en 100 tekens zijn")]
        string name,

        [Required(ErrorMessage = "Type is verplicht")]
        string type,

        [Required(ErrorMessage = "Alcoholpercentage is verplicht")]
        [RegularExpression(@"^(100(\.0)?|([0-9]{1,2})(\.[0-9])?)%$", ErrorMessage = "Ongeldig formaat. Gebruik bijv. '5%', '5.5%' of '100%'")]
        string abv,

        [Required(ErrorMessage = "Afbeelding is verplicht")]
        IFormFile image
    );

    [ApiController]
    [Route("api/beer")]
    public class BeerController : ControllerBase
    {
        private readonly BierdexDBContext _context;
        private readonly BeerService _beerService;
        private readonly ImageService _imageservice;

        // The DbContext is injected via the constructor
        public BeerController(BierdexDBContext context, BeerService beerService, ImageService imageservice)
        {
            _context = context;
            _beerService = beerService;
            _imageservice = imageservice;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Beer>>> GetAllBeers()
        {
            // Use ToListAsync to keep the API responsive and non-blocking
            var beers = await _context.Beers
                .Where(b => b.approved == true)
                .ToListAsync();

            return Ok(beers);
        }

        [HttpGet("all-admin")]
        public async Task<ActionResult<IEnumerable<Beer>>> GetAllBeersAdmin()
        {
            // Use ToListAsync to keep the API responsive and non-blocking
            var beers = await _context.Beers
                .ToListAsync();

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

        [HttpPost("upload-beer")]
        [Authorize]
        public async Task<ActionResult<Beer>> CreateBeer([FromForm] BeerCreateRequest beer)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isSupplier = User.IsInRole("Supplier");

            string savedImagePath = "/uploads/default-beer.png"; // Standaard pad

            // 1. Check of er een afbeelding is geüpload en sla deze op
            if (beer.image != null)
            {
                try
                {
                    // Gebruik de service die we eerder hebben gemaakt
                    // We slaan bier-afbeeldingen op in de map "beers"
                    savedImagePath = await _imageservice.UploadImageAsync(beer.image, "beers");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Fout bij uploaden afbeelding: {ex.Message}");
                }
            }

            var newBeer = new Beer
            {
                name = beer.name,
                type = beer.type,
                abv = beer.abv.ToString(),
                imagePath = savedImagePath,
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

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveBeer(int id)
        {

            var result = await _beerService.ApproveBeerAsync(id);

            if (!result.Success) return Unauthorized(result.Message);
            return Ok(result.Beer);
        }
        [HttpGet("{slug}")]
        public async Task<ActionResult<Beer>> GetBeerBySlug(string slug)
        {
            // Use ToListAsync to keep the API responsive and non-blocking
            var beer = await _context.Beers
                .Where(b => b.approved == true && b.slug == slug)
                .ToListAsync();

            return Ok(beer);
        }
    }
}
