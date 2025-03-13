using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgentScrum.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        UserManager<IdentityUser> userManager,
        ILogger<AdminController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    [Authorize(Policy = "CanManageUsers")]
    public IActionResult ManageUsers()
    {
        // This action is only accessible to users with the "CanManageUsers" policy
        // which requires both the Admin role and the "ManageUsers" permission claim
        return View();
    }
    
    [Authorize(Policy = "CanViewReports")]
    public IActionResult Reports()
    {
        // This action is only accessible to users with the "CanViewReports" policy
        // which requires both the Admin role and the "ViewReports" permission claim
        return View();
    }
    
    // You can also combine role-based and policy-based authorization
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "CanManageUsers")]
    public IActionResult AdvancedUserManagement()
    {
        // This requires both the Admin role AND the CanManageUsers policy
        return View();
    }
    
    // Super admin only action
    [Authorize(Policy = "SuperAdminOnly")]
    public IActionResult SystemConfiguration()
    {
        // This action is only accessible to the super admin (specific email)
        return View();
    }
    
    // Add more admin-only actions here
} 