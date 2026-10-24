using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Atelier.Data;

public static class RolesInitializer
{
    public static async Task InitializeAsync(UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager, IConfiguration configuration, bool seedUsers = false)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);
        foreach (var roleName in Enum.GetNames<UserRole>())
            if (!await roleManager.RoleExistsAsync(roleName))
                Ensure(await roleManager.CreateAsync(new IdentityRole<int>(roleName)));

        // Optional development accounts. Passwords must come from user secrets, never source code.
        if (!seedUsers) return;
        foreach (var role in new[] { UserRole.Admin, UserRole.Staff })
        {
            var section = configuration.GetSection($"SeedUsers:{role}");
            var email = section["Email"];
            var password = section["Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) continue;
            var user = await CreateUser(userManager, email, section["Prenom"] ?? role.ToString(),
                section["Nom"] ?? "Atelier", password, role.ToString());
            await AddRole(userManager, user, role.ToString());
        }
    }

    public static async Task<ApplicationUser> CreateUser(UserManager<ApplicationUser> userManager,
        string email, string prenom, string nom, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null) return user;
        user = new ApplicationUser
        {
            UserName = email, Email = email, Prenom = prenom, Nom = nom,
            Role = Enum.Parse<UserRole>(role)
        };
        Ensure(await userManager.CreateAsync(user, password));
        return user;
    }

    private static async Task AddRole(UserManager<ApplicationUser> users, ApplicationUser user, string role)
    {
        if (!await users.IsInRoleAsync(user, role)) Ensure(await users.AddToRoleAsync(user, role));
    }

    private static async Task AddClaims(UserManager<ApplicationUser> users, ApplicationUser user, params Claim[] claims)
    {
        var existing = await users.GetClaimsAsync(user);
        foreach (var claim in claims)
            if (!existing.Any(current => current.Type == claim.Type && current.Value == claim.Value))
                Ensure(await users.AddClaimAsync(user, claim));
    }

    private static void Ensure(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
