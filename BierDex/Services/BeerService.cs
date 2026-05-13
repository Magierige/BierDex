using BierDex.Data;
using BierDex.Models;

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
    }
}