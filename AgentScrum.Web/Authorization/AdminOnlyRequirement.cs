using Microsoft.AspNetCore.Authorization;

namespace AgentScrum.Web.Authorization;

// Define a custom requirement
public class AdminOnlyRequirement : IAuthorizationRequirement
{
    // You can add properties here if needed
    public string AdminEmail { get; }
    
    public AdminOnlyRequirement(string adminEmail)
    {
        AdminEmail = adminEmail;
    }
}

// Define a handler for the requirement
public class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AdminOnlyRequirement requirement)
    {
        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            return Task.CompletedTask;
        }
        
        // Check if user is in Admin role
        if (!context.User.IsInRole("Admin"))
        {
            return Task.CompletedTask;
        }
        
        // Check if user has the specific admin email (super admin)
        var userEmail = context.User.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
        if (userEmail == requirement.AdminEmail)
        {
            // Mark the requirement as succeeded for this specific admin
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
} 