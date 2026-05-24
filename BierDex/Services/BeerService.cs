using BierDex.Data;
using BierDex.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace BierDex.Services
{
    public class BeerService
    {
        private readonly BierdexDBContext _context;

        public BeerService(BierdexDBContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, Beer? Beer)> UpdateBeerAsync(int id, Beer updatedBeer, string userId, bool isAdmin)
        {
            var beer = await _context.Beers.FindAsync(id);
            if (beer == null) return (false, "Bier niet gevonden.", null);

            // Check: Is de gebruiker de eigenaar OF een admin?
            if (beer.userId != userId && !isAdmin)
                return (false, "Je hebt geen toestemming om dit bier te bewerken.", null);

            // Update velden
            beer.name = updatedBeer.name;
            beer.type = updatedBeer.type;
            beer.abv = updatedBeer.abv;
            beer.imagePath = updatedBeer.imagePath;

            await _context.SaveChangesAsync();
            return (true, "Bier succesvol bijgewerkt.", beer);
        }

        public async Task<(bool Success, string Message)> DeleteBeerAsync(int id, string userId, bool isAdmin)
        {
            var beer = await _context.Beers.FindAsync(id);
            if (beer == null) return (false, "Bier niet gevonden.");

            if (beer.userId != userId && !isAdmin)
                return (false, "Je hebt geen toestemming om dit bier te verwijderen.");

            _context.Beers.Remove(beer);
            await _context.SaveChangesAsync();
            return (true, "Bier succesvol verwijderd.");
        }

        public async Task<(bool Success, string Message, Beer? Beer)> CreateBeerAsync(Beer newBeer, string userId, bool isAdmin, bool isSupplier)
        {
            if (!isAdmin && !isSupplier)
                return (false, "Je hebt geen toestemming om een bier aan te maken.", null);

            newBeer.userId = userId;
            newBeer.slug = await CreateUniqueSlug(newBeer.name);
            _context.Beers.Add(newBeer);
            await _context.SaveChangesAsync();
            return (true, "Bier succesvol aangemaakt.", newBeer);
        }

        public async Task<(bool Success, string Message, Beer? Beer)> ApproveBeerAsync(int id)
        {
            var beer = await _context.Beers.FindAsync(id);
            if (beer == null) return (false, "Bier niet gevonden.", null);

            beer.approved = true;
            await _context.SaveChangesAsync();
            return (true, "Bier succesvol goedgekeurd.", beer);
        }

        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";

            string str = phrase.ToLower();
            // Verwijder ongeldige tekens
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // Zet witruimte om naar enkel streepje
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // Spaties naar streepjes
            str = str.Replace(" ", "-");

            return str;
        }

        public async Task<string> CreateUniqueSlug(string name)
        {
            string slug = GenerateSlug(name);
            var existingBeers = await _context.Beers
                .Where(b => b.slug == slug || b.slug.StartsWith(slug + "-"))
                .Select(b => b.slug)
                .ToListAsync();

            if (!existingBeers.Contains(slug))
            {
                return slug;
            }

            // Als de slug al bestaat, voeg een nummertje toe
            int i = 1;
            while (existingBeers.Contains($"{slug}-{i}"))
            {
                i++;
            }

            return $"{slug}-{i}";
        }

        public async Task<double> UpdateAndGetBeerAverageRatingAsync(int beerId)
        {
            var beer = await _context.Beers.FindAsync(beerId);
            if (beer == null) return 0.0;

            var hasReviews = await _context.Reviews.AnyAsync(r => r.BeerId == beerId);
            if (!hasReviews)
            {
                beer.rating = 0.0;
                await _context.SaveChangesAsync();
                return 0.0;
            }

            double average = await _context.Reviews
                .Where(r => r.BeerId == beerId)
                .AverageAsync(r => r.Rating);

            // Save the 1-decimal average back to the Beer record
            beer.rating = Math.Round(average, 1);
            await _context.SaveChangesAsync();

            return beer.rating;
        }
    }
}