using Microsoft.AspNetCore.Identity;

namespace BierDex.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Supplier", "User" };

            // 1. Create roles
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Create users
            await CreateUser(userManager, "admin", "admin@test.com", "Password123!", "Admin");
            await CreateUser(userManager, "supplier", "supplier@test.com", "Password123!", "Supplier");
            await CreateUser(userManager, "user", "user@test.com", "Password123!", "User");
        }

        private static async Task CreateUser(
            UserManager<IdentityUser> userManager,
            string username,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
