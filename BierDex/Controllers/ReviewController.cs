using Azure.Core;
using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace BierDex.Controllers
{
    public record ReviewCreateRequest(string Content, int Rating, int BeerId);

    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly BierdexDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewController(BierdexDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

            return CreatedAtAction(nameof(GetReviewByBeerId), new { id = review.BeerId }, review);
        }
    }
}