using Atelier.Enums;
using Atelier.Modeles;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Atelier.Data
{
    public static class RolesInitializer
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (userManager == null || roleManager == null)
            {
                throw new ArgumentNullException("userManager ou roleManager est null");
            }
            var roleNames = Enum.GetValues(typeof(UserRole));
            foreach (var role in roleNames)
            {
                string roleName = role.ToString();
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = await CreateUser(userManager, "admin@atelier.com", "Admin", "User", "Admin123!", UserRole.Admin.ToString());
            await AddRole(userManager, adminUser, UserRole.Admin.ToString());
            var staffUser = await CreateUser(userManager, "staff@atelier.com", "Staff", "User", "Staff123!", UserRole.Staff.ToString());
            await AddRole(userManager, staffUser, UserRole.Staff.ToString());

        }

        public static async Task<ApplicationUser> CreateUser(UserManager<ApplicationUser> userManager, string email, string prenom, string nom, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Prenom = prenom,
                    Nom = nom
                };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception($"Erreur lors de la création de l'utilisateur {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            return user;
        }

        private static async Task AddRole(UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
        {
            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains(role))
            {
                var result = await userManager.AddToRoleAsync(user, role);
                if (!result.Succeeded)
                {
                    throw new Exception($"Erreur lors de l'ajout de l'utilisateur {user.Email} au rôle {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async Task AddClaims(UserManager<ApplicationUser> userManager, ApplicationUser user,
            params Claim[] claims)
        {
            var existingClaims = await userManager.GetClaimsAsync(user);
            foreach (var claim in existingClaims)
            {
                if (!existingClaims.Any(claim => claim.Type == claim.Type && claim.Value == claim.Value))
                {
                    await userManager.AddClaimAsync(user, claim);
                }
            }
        }
    }
}
