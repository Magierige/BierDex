using Microsoft.EntityFrameworkCore;
using BierDex.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BierDex.Data
{
    public class BierdexDBContext : IdentityDbContext
    {
        public BierdexDBContext(DbContextOptions<BierdexDBContext> options)
            : base(options)
        {
        }

        public DbSet<Beer> Beers { get; set; }

       
    }
}




