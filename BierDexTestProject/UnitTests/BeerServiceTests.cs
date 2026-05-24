using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace BierDex.Tests.UnitTests;

[TestFixture]
public class BeerServiceTests
{
    private BierdexDBContext _context;
    private BeerService _beerService;
    private Beer _testBeer;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BierdexDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BierdexDBContext(options);
        _beerService = new BeerService(_context);

        // Seed a default test beer to use across multiple test instances
        _testBeer = new Beer
        {
            Id = 1,
            name = "Hertog Jan Grand Prestige",
            barcode = "12345678",
            type = "Gerstewijn",
            abv = "10%",
            imagePath = "/images/grandprestige.png",
            slug = "hertog-jan-grand-prestige",
            userId = "owner-123",
            rating = 0.0,
            approved = false
        };

        _context.Beers.Add(_testBeer);
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // --- Slug Tests ---

    [Test]
    public void GenerateSlug_ShouldConvertToLowercaseAndReplaceSpaces()
    {
        string bierNaam = "Hertog Jan Tripel!";

        string result = BeerService.GenerateSlug(bierNaam);

        Assert.That(result, Is.EqualTo("hertog-jan-tripel"));
    }

    [Test]
    public async Task CreateUniqueSlug_ShouldAppendNumber_IfSlugAlreadyExists()
    {
        var uniqueSlug = await _beerService.CreateUniqueSlug("Hertog Jan Grand Prestige");

        Assert.That(uniqueSlug, Is.EqualTo("hertog-jan-grand-prestige-1"));
    }

    // --- Create Beer Tests ---

    [Test]
    public async Task CreateBeerAsync_UserNotAdminOrSupplier_ShouldReturnFalse()
    {
        var newBeer = new Beer { name = "Brand Up" };

        var result = await _beerService.CreateBeerAsync(newBeer, "user-456", isAdmin: false, isSupplier: false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Je hebt geen toestemming om een bier aan te maken."));
    }

    [Test]
    public async Task CreateBeerAsync_UserIsSupplier_ShouldSuccessfullyCreateBeerWithUniqueSlug()
    {
        var newBeer = new Beer { name = "Hertog Jan Grand Prestige", type = "Gerstewijn", abv = "10%", barcode = "12345678", imagePath = "iamages/Despo.png" };

        var result = await _beerService.CreateBeerAsync(newBeer, "supplier-1", isAdmin: false, isSupplier: true);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Beer, Is.Not.Null);
        Assert.That(result.Beer.slug, Is.EqualTo("hertog-jan-grand-prestige-1"));
        Assert.That(result.Beer.userId, Is.EqualTo("supplier-1"));
    }

    // --- Update Beer Tests ---

    [Test]
    public async Task UpdateBeerAsync_UserIsNotOwnerAndNotAdmin_ShouldReturnFalse()
    {
        var updatedData = new Beer { name = "Hertog Jan Grand Prestige Modified" };

        var result = await _beerService.UpdateBeerAsync(1, updatedData, userId: "stranger-id", isAdmin: false);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Is.EqualTo("Je hebt geen toestemming om dit bier te bewerken."));
    }

    [Test]
    public async Task UpdateBeerAsync_UserIsOwner_ShouldSuccessfullyUpdateFields()
    {
        var updatedData = new Beer
        {
            name = "Hertog Jan Grand Prestige 2026",
            type = "Gerstewijn-Gepast",
            abv = "10.5%",
            imagePath = "/images/gp2026.png"
        };

        var result = await _beerService.UpdateBeerAsync(1, updatedData, userId: "owner-123", isAdmin: false);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Beer!.name, Is.EqualTo("Hertog Jan Grand Prestige 2026"));
        Assert.That(result.Beer!.type, Is.EqualTo("Gerstewijn-Gepast"));
    }

    // --- Delete Beer Tests ---

    [Test]
    public async Task DeleteBeerAsync_UserIsAdmin_ShouldSuccessfullyDeleteBeer()
    {
        var result = await _beerService.DeleteBeerAsync(1, userId: "some-admin", isAdmin: true);

        Assert.That(result.Success, Is.True);
        var dbBeer = await _context.Beers.FindAsync(1);
        Assert.That(dbBeer, Is.Null);
    }

    // --- Approve Beer Tests ---

    [Test]
    public async Task ApproveBeerAsync_ExistingBeer_ShouldSetApprovedToTrue()
    {
        var result = await _beerService.ApproveBeerAsync(1);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Beer!.approved, Is.True);
    }

    // --- Average Rating Calculation Tests ---

    [Test]
    public async Task UpdateAndGetBeerAverageRatingAsync_NoReviews_ShouldReturnZero()
    {
        var rating = await _beerService.UpdateAndGetBeerAverageRatingAsync(1);

        Assert.That(rating, Is.EqualTo(0.0));
    }

    [Test]
    public async Task UpdateAndGetBeerAverageRatingAsync_WithMultipleReviews_ShouldReturnRoundedAverage()
    {
        var user = new IdentityUser { Id = "reviewer", UserName = "Bob" };

        // Add reviews: 4 stars, 5 stars, and 5 stars -> (14 / 3) = 4.666... -> Rounded to 4.7
        var review1 = new Review("Goed", 4, user, _testBeer);
        var review2 = new Review("Super", 5, user, _testBeer);
        var review3 = new Review("Geweldig", 5, user, _testBeer);

        _context.Reviews.AddRange(review1, review2, review3);
        await _context.SaveChangesAsync();

        var averageRating = await _beerService.UpdateAndGetBeerAverageRatingAsync(1);

        Assert.That(averageRating, Is.EqualTo(4.7));

        var dbBeer = await _context.Beers.FindAsync(1);
        Assert.That(dbBeer!.rating, Is.EqualTo(4.7));
    }
}