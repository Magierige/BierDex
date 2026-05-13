using BierDex.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BierDex.Data
{
    public static class BeerSeeder
    {
        public static async Task SeedAsync(
            BierdexDBContext context,
            UserManager<IdentityUser> userManager)
        {
            if (context.Beers.Any())
                return; // already seeded

            var user = await userManager.FindByEmailAsync("supplier@test.com");
            var user2 = await userManager.FindByEmailAsync("supplier2@test.com");

            if (user == null || user2 == null)
                return; // safety check

            var beers = new List<Beer>
            {
                new Beer(1001, "Heineken", "Lager", "/uploads/beers/default-beer.png", "5%", user),
                new Beer(1002, "Amstel", "Lager", "/uploads/beers/default-beer.png", "5%", user),
                new Beer(1003, "Duvel", "Belgian Strong Ale", "/uploads/beers/default-beer.png", "8.5%", user),
                new Beer(1004, "Leffe Blonde", "Abbey Beer", "/uploads/beers/default-beer.png", "6.6%", user),
                new Beer(1005, "Corona", "Pale Lager", "/uploads/beers/default-beer.png", "4.5%", user),
                new Beer(1006, "Guinness", "Stout", "/uploads/beers/default-beer.png", "4.2%", user2),
                new Beer(1007, "Hoegaarden", "Wheat Beer", "/uploads/beers/default-beer.png", "4.9%", user2),
                new Beer(1008, "Chimay Blue", "Trappist Ale", "/uploads/beers/default-beer.png", "9%", user2),
                new Beer(1009, "Paulaner Hefe-Weissbier", "Wheat Beer", "/uploads/beers/default-beer.png", "5.5%", user2),
                new Beer(1010, "BrewDog Punk IPA", "IPA", "/uploads/beers/default-beer.png", "5.6%", user2)
            };
            for (int i = 0; i < beers.Count; i++)
            {
                beers[i].approved = true; // Ensure beers are approved
            }

            context.Beers.AddRange(beers);
            await context.SaveChangesAsync();
        }
    }
}
