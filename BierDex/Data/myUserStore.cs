using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using BierDex.Data;

public class MyUserStore : UserStore<IdentityUser, IdentityRole, BierdexDBContext>
{
    public MyUserStore(BierdexDBContext context, IdentityErrorDescriber describer = null)
        : base(context, describer) { }

    public override async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken = default)
    {
        // 1. Call the base implementation to actually save the user to the database
        var result = await base.CreateAsync(user, cancellationToken);

        if (result.Succeeded)
        {
            // 2. Find the "User" role ID from the database
            var userRole = Context.Roles.FirstOrDefault(r => r.Name == "User");

            if (userRole != null)
            {
                // 3. Manually add the entry to the UserRoles join table
                Context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = user.Id,
                    RoleId = userRole.Id
                });

                await Context.SaveChangesAsync(cancellationToken);
            }
        }

        return result;
    }
}