using BierDex.Data;
using BierDex.Models;
using BierDex.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace BierDex.Tests.UnitTests;

[TestFixture]
public class BeerServiceTests
{
    private BierdexDBContext _context;
    private BeerService _beerService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BierdexDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BierdexDBContext(options);
        _beerService = new BeerService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

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
        var options = new DbContextOptionsBuilder<BierdexDBContext>()
            .UseInMemoryDatabase(databaseName: "SlugTestDatabase_" + Guid.NewGuid().ToString())
            .Options;

        using var context = new BierdexDBContext(options);

        var bestaandBier = new Beer
        {
            name = "Duvel",
            slug = "duvel",
            userId = "1",
            barcode = "123456789",
            imagePath = "dummy.jpg",
            type = "Blond",
            abv = "8.5%"
        };

        context.Beers.Add(bestaandBier);
        await context.SaveChangesAsync(); // Sla op zodat CreateUniqueSlug het bier kan vinden

        var beerService = new BeerService(context);

        var uniqueSlug = await beerService.CreateUniqueSlug("Duvel");

        Assert.That(uniqueSlug, Is.EqualTo("duvel-1"));
    }
}