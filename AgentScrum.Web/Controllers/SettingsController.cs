using System.Text.Json;
using AgentScrum.Web.Data;
using AgentScrum.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgentScrum.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ILogger<SettingsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var credentials = await _context.GoogleDriveCredentials
            .FirstOrDefaultAsync(c => c.UserId == userId);
        
        return View(credentials);
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadGoogleCredentials(IFormFile credentialsFile)
    {
        if (credentialsFile == null || credentialsFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a file to upload.");
            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User);
        
        try
        {
            // Read the file content
            using var streamReader = new StreamReader(credentialsFile.OpenReadStream());
            var jsonContent = await streamReader.ReadToEndAsync();
            
            // Validate JSON format
            var isValidJson = IsValidGoogleCredentialsJson(jsonContent);
            if (!isValidJson)
            {
                ModelState.AddModelError("", "The uploaded file is not a valid Google credentials JSON file.");
                return RedirectToAction(nameof(Index));
            }
            
            // Check if user already has credentials
            var existingCredentials = await _context.GoogleDriveCredentials
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (existingCredentials != null)
            {
                // Update existing credentials
                existingCredentials.CredentialsJson = jsonContent;
                existingCredentials.IsValid = true;
                existingCredentials.UploadedAt = DateTime.UtcNow;
                existingCredentials.LastValidatedAt = DateTime.UtcNow;
                
                _context.GoogleDriveCredentials.Update(existingCredentials);
            }
            else
            {
                // Create new credentials
                var newCredentials = new GoogleDriveCredentials
                {
                    UserId = userId,
                    CredentialsJson = jsonContent,
                    IsValid = true,
                    UploadedAt = DateTime.UtcNow,
                    LastValidatedAt = DateTime.UtcNow
                };
                
                _context.GoogleDriveCredentials.Add(newCredentials);
            }
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Google Drive credentials uploaded successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Google Drive credentials");
            ModelState.AddModelError("", "An error occurred while uploading the credentials.");
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    private bool IsValidGoogleCredentialsJson(string jsonContent)
    {
        try
        {
            // Try to parse the JSON
            var jsonDocument = JsonDocument.Parse(jsonContent);
            var root = jsonDocument.RootElement;
            
            // Check for required fields in Google credentials JSON
            return root.TryGetProperty("type", out var type) &&
                   root.TryGetProperty("project_id", out _) &&
                   root.TryGetProperty("private_key_id", out _) &&
                   root.TryGetProperty("private_key", out _) &&
                   root.TryGetProperty("client_email", out _) &&
                   type.GetString() == "service_account";
        }
        catch
        {
            return false;
        }
    }
} 