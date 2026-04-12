using Microsoft.EntityFrameworkCore;
using BierDex.Models;

namespace BierDex.Data
{
    public class BierdexDBContext(DbContextOptions<BierdexDBContext> options) : DbContext(options)
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().HasData(
                new(1, "Admin"),
                new(2, "Suplier"),
                new(3, "User")
            );
            modelBuilder.Entity<User>().HasData(
                new(1, "beheerder@test.com", "password123", "Beheerder", 1),
                new(2, "hertogjan@test.com", "password123", "Hertogjan", 2),
                new(3, "ronald@test.com", "password123", "Roanld", 3)
            );
        }
    }
}



    
