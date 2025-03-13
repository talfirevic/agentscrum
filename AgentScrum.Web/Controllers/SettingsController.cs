using System.Text.Json;
using AgentScrum.Web.Adapters.Contracts.GoogleDocs;
using AgentScrum.Web.Data;
using AgentScrum.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgentScrum.Web.Controllers;

[Authorize]
public class SettingsController(
    ApplicationDbContext context,
    UserManager<IdentityUser> userManager,
    ILogger<SettingsController> logger,
    IGoogleDriveAdapter googleDriveAdapter)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        // Clear ModelState to avoid duplicate error messages
        ModelState.Clear();
        
        var userId = userManager.GetUserId(User);
        var credentials = await context.GoogleDriveCredentials
            .FirstOrDefaultAsync(c => c.UserId == userId);

        switch (credentials)
        {
            // Check if credentials exist but are not valid
            case { IsValid: false }:
                ViewData["CredentialError"] = "Your Google Drive credentials are invalid. Please upload a valid credentials file.";
                break;
            // Check if credentials are valid but not loaded in the adapter
            case { IsValid: true } when !googleDriveAdapter.HasValidCredentials():
                ViewData["CredentialWarning"] = "Your credentials are valid but not currently loaded. Please try refreshing the page or contact support if the issue persists.";
                break;
            // Check if no credentials exist
            case null:
                ViewData["CredentialInfo"] = "You haven't uploaded Google Drive credentials yet. Some features may not work until you upload valid credentials.";
                break;
        }
        
        return View(credentials);
    }
    
    [HttpPost]
    public async Task<IActionResult> UploadGoogleCredentials(IFormFile? credentialsFile)
    {
        // Clear ModelState to avoid duplicate error messages
        ModelState.Clear();
        
        if (credentialsFile == null || credentialsFile.Length == 0)
        {
            // Only set ViewData, don't add to ModelState
            ViewData["CredentialError"] = "Please select a file to upload.";
            
            var currentUserId = userManager.GetUserId(User);
            var credentials = await context.GoogleDriveCredentials
                .FirstOrDefaultAsync(c => c.UserId == currentUserId);
            
            return View("Index", credentials);
        }

        var userId = userManager.GetUserId(User);
        
        try
        {
            // Read the file content
            using var streamReader = new StreamReader(credentialsFile.OpenReadStream());
            var jsonContent = await streamReader.ReadToEndAsync();
            
            // Validate JSON format using the adapter
            var isValidJson = googleDriveAdapter.IsValidGoogleCredentialsJson(jsonContent);
            if (!isValidJson)
            {
                // Only set ViewData, don't add to ModelState
                ViewData["CredentialError"] = "The uploaded file is not a valid Google credentials JSON file. Please check the file and try again.";
                
                var credentials = await context.GoogleDriveCredentials
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                return View("Index", credentials);
            }
            
            // Check if user already has credentials
            var existingCredentials = await context.GoogleDriveCredentials
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (existingCredentials != null)
            {
                // Update existing credentials
                existingCredentials.CredentialsJson = jsonContent;
                existingCredentials.IsValid = true;
                existingCredentials.UploadedAt = DateTime.UtcNow;
                existingCredentials.LastValidatedAt = DateTime.UtcNow;
                
                context.GoogleDriveCredentials.Update(existingCredentials);
            }
            else
            {
                // Create new credentials
                var newCredentials = new GoogleDriveCredentials
                {
                    UserId = userId!,
                    CredentialsJson = jsonContent,
                    IsValid = true,
                    UploadedAt = DateTime.UtcNow,
                    LastValidatedAt = DateTime.UtcNow,
                    Id = 0,
                };
                
                context.GoogleDriveCredentials.Add(newCredentials);
            }
            
            await context.SaveChangesAsync();
            
            // Try to set the credentials on the adapter
            try
            {
                googleDriveAdapter.SetCredentials(jsonContent);
                TempData["SuccessMessage"] = "Google Drive credentials uploaded and validated successfully.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting credentials on the adapter");
                ViewData["CredentialWarning"] = "Credentials were saved but could not be loaded. Please try refreshing the page.";
                
                var credentials = await context.GoogleDriveCredentials
                    .FirstOrDefaultAsync(c => c.UserId == userId);
                
                return View("Index", credentials);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading Google Drive credentials");
            // Only set ViewData, don't add to ModelState
            ViewData["CredentialError"] = "An error occurred while uploading the credentials. Please try again.";
            
            var credentials = await context.GoogleDriveCredentials
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            return View("Index", credentials);
        }
        
        return RedirectToAction(nameof(Index));
    }
} 