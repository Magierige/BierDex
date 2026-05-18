using BierDex.Data;
using BierDex.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Net;
using System.Net.Http.Json;

namespace BierDex.Tests.IntegrationTests;

[TestFixture]
public class BeerControllerTests
{
    private CustomWebApplicationFactory _factory;
    private HttpClient _client;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [SetUp]
    public async Task Setup()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BierdexDBContext>();

        context.Beers.RemoveRange(context.Beers);
        context.Reviews.RemoveRange(context.Reviews);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task GetAllBeers_ShouldReturnOnlyApprovedBeers()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<BierdexDBContext>();
            context.Beers.AddRange(new List<Beer>
            {
                new Beer
                {
                    name = "Goedgekeurd Bier",
                    approved = true,
                    abv = "5%",
                    slug = "b1",
                    userId = "1",
                    barcode = "111111111", 
                    imagePath = "b1.jpg",   
                    type = "Blond"         
                },
                new Beer
                {
                    name = "In Afwachting Bier",
                    approved = false,
                    abv = "6%",
                    slug = "b2",
                    userId = "1",
                    barcode = "222222222", 
                    imagePath = "b2.jpg",   
                    type = "Dubbel"        
                }
            });
            await context.SaveChangesAsync();
        }

        // Act
        var response = await _client.GetAsync("/api/beer/all");

        // Assert
        response.EnsureSuccessStatusCode();
        var beers = await response.Content.ReadFromJsonAsync<List<Beer>>();

        Assert.That(beers, Is.Not.Null);
        Assert.That(beers!.Count, Is.EqualTo(1));
        Assert.That(beers[0].name, Is.EqualTo("Goedgekeurd Bier"));
    }

    [Test]
    public async Task GetUserBeers_WithoutAuthToken_ShouldReturn401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/beer/my-beers");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}