using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AgentScrum.Web.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, string adminEmail, string adminPassword)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            
            await SeedRoles(roleManager);
            await SeedAdminUser(userManager, adminEmail, adminPassword);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database with roles and admin user.");
        }
    }
    
    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        // Check if roles already exist and exit if there are
        if (await roleManager.RoleExistsAsync("Admin"))
            return;
            
        // Create Admin role
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    
    private static async Task SeedAdminUser(UserManager<IdentityUser> userManager, string adminEmail, string adminPassword)
    {
        // Check if admin user exists
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            // Create admin user
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Skip email confirmation for admin
            };
            
            var result = await userManager.CreateAsync(adminUser, adminPassword);
            
            if (result.Succeeded)
            {
                // Add admin user to Admin role
                await userManager.AddToRoleAsync(adminUser, "Admin");
                
                // Add admin claims
                await AddAdminClaims(userManager, adminUser);
            }
        }
        else
        {
            // Ensure existing user is in admin role
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            
            // Ensure admin has all required claims
            await AddAdminClaims(userManager, adminUser);
        }
    }
    
    private static async Task AddAdminClaims(UserManager<IdentityUser> userManager, IdentityUser adminUser)
    {
        // Define the claims that an admin should have
        var adminClaims = new List<Claim>
        {
            new Claim("Permission", "ManageUsers"),
            new Claim("Permission", "ViewReports"),
            new Claim("Permission", "ManageSettings")
        };
        
        // Get existing claims
        var existingClaims = await userManager.GetClaimsAsync(adminUser);
        
        // Add any missing claims
        foreach (var claim in adminClaims)
        {
            if (!existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value))
            {
                await userManager.AddClaimAsync(adminUser, claim);
            }
        }
    }
} 