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
    public record ReviewCreateRequest(
        [Required(ErrorMessage = "Review mag niet leeg zijn.")]
        [StringLength(1000, MinimumLength = 5, ErrorMessage = "De review moet tussen de 5 en 1000 tekens lang zijn.")]
        string Content,

        [Required]
        [Range(1, 5, ErrorMessage = "Rating moet tussen 1 en 5 sterren liggen.")]
        int Rating,

        [Required]
        int BeerId
    );

    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly BierdexDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BeerService _beerService;

        public ReviewController(BierdexDBContext context, UserManager<IdentityUser> userManager, BeerService beerService)
        {
            _context = context;
            _userManager = userManager;
            _beerService = beerService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Review>>> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Beer)
                .Include(r => r.User)
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("beer/{id}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewByBeerId(int id)
        {
            // Use .Where() to filter all reviews and .ToListAsync() to execute the query
            var reviews = await _context.Reviews
                .Include(r => r.Beer)
                .Include(r => r.User)
                .Where(r => r.BeerId == id)
                .ToListAsync();

            // Even if no reviews are found, an empty list [] is usually 
            // better than a 404 for a collection endpoint.
            return Ok(reviews);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<ActionResult<Review>> CreateReview([FromBody] ReviewCreateRequest requestReview)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found.");

            var beer = await _context.Beers.FindAsync(requestReview.BeerId);
            if (beer == null) return NotFound("Beer not found.");

            // Call the constructor you defined in the Review model
            // This will correctly set Username, UserId, BeerId, etc.
            var review = new Review(
                requestReview.Content,
                requestReview.Rating,
                user,
                beer
            );

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await _beerService.UpdateAndGetBeerAverageRatingAsync(beer.Id);

            return CreatedAtAction(nameof(GetReviewByBeerId), new { id = review.BeerId }, review);
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            // Haal de huidige ingelogde User ID op uit de claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("Gebruiker is niet geautoriseerd.");

            // Zoek de review op inclusief de User data om de eigenaar te controleren
            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound("Review niet gevonden.");

            // Check de rechten: Is de gebruiker een Admin OF de eigenaar van de review?
            var isAdmin = User.IsInRole("Admin");
            var isOwner = review.UserId == userId || (review.User != null && review.User.Id == userId);

            if (!isAdmin && !isOwner)
            {
                return Forbid("Je bent niet de eigenaar van deze review en geen admin.");
            }

            // Verwijder de review en sla de wijzigingen op
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // 204 No Content is de standaard succesvolle HTTP statuscode voor een DELETE actie
            return NoContent();
        }
    }
}