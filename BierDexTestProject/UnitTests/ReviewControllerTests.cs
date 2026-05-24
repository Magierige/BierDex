using BierDex.Controllers;
using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NuGet.ContentModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BierDex.Tests.UnitTests;

[TestFixture]
public class ReviewControllerTests
{
    private BierdexDBContext _context;
    private Mock<UserManager<IdentityUser>> _mockUserManager;
    private ReviewController _controller;
    private IdentityUser _testUser;
    private Beer _testBeer;
    private ClaimsPrincipal _claimsPrincipal;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BierdexDBContext>()
            .UseInMemoryDatabase(databaseName: $"BierDexReviewTest_{System.Guid.NewGuid()}")
            .Options;
        _context = new BierdexDBContext(options);

        var userStoreMock = new Mock<IUserStore<IdentityUser>>();
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _testUser = new IdentityUser { Id = "user-123", UserName = "Ronald" };

        _testBeer = new Beer
        {
            Id = 1,
            name = "Hertog Jan Grand Prestige",
            barcode = "12345678",
            type = "Gerstewijn",
            abv = "10%",
            imagePath = "/images/grandprestige.png",
            slug = "hertog-jan-grand-prestige",
            userId = "admin-id",
            rating = 0.0
        };

        _context.Beers.Add(_testBeer);
        _context.SaveChanges();

        // Using a real BeerService since we are using InMemoryDatabase
        var beerService = new BeerService(_context);

        _controller = new ReviewController(_context, _mockUserManager.Object, beerService);

        // Setup default user context (Regular Owner)
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _claimsPrincipal }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SetupUserRole(string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
    }

    [Test]
    public async Task GetAllReviews_ShouldReturnAllReviewsFromDatabase()
    {
        var review = new Review("Lekker biertje!", 5, _testUser, _testBeer);
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAllReviews();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var reviews = okResult.Value as IEnumerable<Review>;
        Assert.That(reviews, Is.Not.Null);
        Assert.That(reviews, Has.Exactly(1).Items);
    }

    [Test]
    public async Task GetReviewByBeerId_NoReviewsFound_ShouldReturnEmptyListWithOk()
    {
        // Act
        var result = await _controller.GetReviewByBeerId(999);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var reviews = okResult.Value as IEnumerable<Review>;
        Assert.That(reviews, Is.Empty);
    }

    [Test]
    public async Task CreateReview_ValidRequest_ShouldReturnCreatedAtAction()
    {
        var request = new ReviewCreateRequest("Heerlijk complex van smaak.", 5, _testBeer.Id);
        _mockUserManager.Setup(um => um.FindByIdAsync("user-123")).ReturnsAsync(_testUser);

        // Act
        var result = await _controller.CreateReview(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        Assert.That(createdResult.ActionName, Is.EqualTo(nameof(_controller.GetReviewByBeerId)));

        var createdReview = createdResult.Value as Review;
        Assert.That(createdReview, Is.Not.Null);
        Assert.That(createdReview.Content, Is.EqualTo("Heerlijk complex van smaak."));
        Assert.That(createdReview.Rating, Is.EqualTo(5));
    }

    [Test]
    public async Task CreateReview_BeerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = new ReviewCreateRequest("Prima pils", 4, 999);
        _mockUserManager.Setup(um => um.FindByIdAsync("user-123")).ReturnsAsync(_testUser);

        // Act
        var result = await _controller.CreateReview(request);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo("Beer not found."));
    }

    [Test]
    public async Task CreateReview_UserNotFoundInUserManager_ShouldReturnNotFound()
    {
        var request = new ReviewCreateRequest("Prima pils", 4, _testBeer.Id);
        _mockUserManager.Setup(um => um.FindByIdAsync("user-123")).ReturnsAsync((IdentityUser)null!);

        // Act
        var result = await _controller.CreateReview(request);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo("User not found."));
    }

    // --- NEW: Tests for DeleteReview ---

    [Test]
    public async Task DeleteReview_ReviewNotFound_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.DeleteReview(999);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.EqualTo("Review niet gevonden."));
    }

    [Test]
    public async Task DeleteReview_UserIsOwner_ShouldReturnNoContent()
    {
        // Arrange
        var review = new Review("Mijn review", 4, _testUser, _testBeer);
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteReview(review.Id);

        // Assert
        Assert.That(result, Is.TypeOf<NoContentResult>());
        var dbReview = await _context.Reviews.FindAsync(review.Id);
        Assert.That(dbReview, Is.Null);
    }

    [Test]
    public async Task DeleteReview_UserIsAdminButNotOwner_ShouldReturnNoContent()
    {
        // Arrange
        var strangerUser = new IdentityUser { Id = "stranger-danger", UserName = "Henk" };
        var review = new Review("Niet mijn review", 3, strangerUser, _testBeer);
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Change current context user to an Admin (still ID 'user-123')
        SetupUserRole("Admin");

        // Act
        var result = await _controller.DeleteReview(review.Id);

        // Assert
        Assert.That(result, Is.TypeOf<NoContentResult>());
        var dbReview = await _context.Reviews.FindAsync(review.Id);
        Assert.That(dbReview, Is.Null);
    }

    [Test]
    public async Task DeleteReview_UserNotOwnerAndNotAdmin_ShouldReturnForbid()
    {
        // Arrange
        var strangerUser = new IdentityUser { Id = "stranger-danger", UserName = "Henk" };
        var review = new Review("Niet mijn review", 3, strangerUser, _testBeer);
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteReview(review.Id);

        // Assert
        var forbidResult = result as ForbidResult;
        Assert.That(forbidResult, Is.Not.Null);

        // Ensure it wasn't accidentally deleted
        var dbReview = await _context.Reviews.FindAsync(review.Id);
        Assert.That(dbReview, Is.Not.Null);
    }
}