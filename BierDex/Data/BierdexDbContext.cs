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

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Belangrijk: Roep altijd base.OnModelCreating aan bij IdentityDbContext!
            base.OnModelCreating(builder);

            // Configureer de Slug kolom
            builder.Entity<Beer>()
                .HasIndex(b => b.slug)
                .IsUnique();

            // Optioneel: Je kunt hier ook extra beperkingen toevoegen
            builder.Entity<Beer>()
                .Property(b => b.slug)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}