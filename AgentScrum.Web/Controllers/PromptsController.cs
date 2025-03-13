using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AgentScrum.Web.Data;
using AgentScrum.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AgentScrum.Web.Controllers
{
    [Authorize(Policy = "RequireAdminRole")]
    public class PromptsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PromptsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Prompts
        public async Task<IActionResult> Index()
        {
            // Get only the current versions of prompts
            var currentPrompts = await _context.Prompts
                .Where(p => p.IsCurrentVersion)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
                
            return View(currentPrompts);
        }

        // GET: Prompts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prompt = await _context.Prompts
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (prompt == null)
            {
                return NotFound();
            }

            // Get all versions of this prompt
            var originalId = prompt.OriginalPromptId ?? prompt.Id;
            ViewBag.Versions = await _context.Prompts
                .Where(p => p.Id == originalId || p.OriginalPromptId == originalId)
                .OrderByDescending(p => p.Version)
                .ToListAsync();

            return View(prompt);
        }

        // GET: Prompts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Prompts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Content,Category")] Prompt prompt)
        {
            // Clear any model errors related to fields we're setting programmatically
            ModelState.Remove("CreatedBy");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Version");
            ModelState.Remove("IsCurrentVersion");
            
            if (ModelState.IsValid)
            {
                // Set initial values for a new prompt
                prompt.Version = 1;
                prompt.CreatedAt = DateTime.UtcNow;
                prompt.CreatedBy = _userManager.GetUserId(User) ?? "Unknown";
                prompt.IsCurrentVersion = true;
                
                _context.Add(prompt);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(prompt);
        }

        // GET: Prompts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound();
            }
            return View(prompt);
        }

        // POST: Prompts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Content,Category")] Prompt promptUpdate)
        {
            if (id != promptUpdate.Id)
            {
                return NotFound();
            }

            // Clear any model errors related to fields we're setting programmatically
            ModelState.Remove("CreatedBy");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Version");
            ModelState.Remove("IsCurrentVersion");
            ModelState.Remove("OriginalPromptId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the existing prompt
                    var existingPrompt = await _context.Prompts.FindAsync(id);
                    if (existingPrompt == null)
                    {
                        return NotFound();
                    }

                    // Create a new version
                    var newVersion = new Prompt
                    {
                        Name = promptUpdate.Name,
                        Description = promptUpdate.Description,
                        Content = promptUpdate.Content,
                        Category = promptUpdate.Category,
                        Version = existingPrompt.Version + 1,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = _userManager.GetUserId(User) ?? "Unknown",
                        IsCurrentVersion = true,
                        OriginalPromptId = existingPrompt.OriginalPromptId ?? existingPrompt.Id
                    };

                    // Update the existing prompt to no longer be the current version
                    existingPrompt.IsCurrentVersion = false;
                    
                    // Add the new version and update the existing one
                    _context.Update(existingPrompt);
                    _context.Add(newVersion);
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromptExists(promptUpdate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(promptUpdate);
        }

        // GET: Prompts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prompt = await _context.Prompts
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (prompt == null)
            {
                return NotFound();
            }

            return View(prompt);
        }

        // POST: Prompts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound();
            }
            
            // If this is the current version of a prompt with multiple versions
            if (prompt.IsCurrentVersion && prompt.OriginalPromptId.HasValue)
            {
                // Find the previous version and make it the current version
                var previousVersion = await _context.Prompts
                    .Where(p => p.OriginalPromptId == prompt.OriginalPromptId && p.Id != prompt.Id)
                    .OrderByDescending(p => p.Version)
                    .FirstOrDefaultAsync();
                    
                if (previousVersion != null)
                {
                    previousVersion.IsCurrentVersion = true;
                    _context.Update(previousVersion);
                }
            }
            
            _context.Prompts.Remove(prompt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Prompts/ViewVersion/5
        public async Task<IActionResult> ViewVersion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound();
            }

            // Get all versions of this prompt
            var originalId = prompt.OriginalPromptId ?? prompt.Id;
            ViewBag.Versions = await _context.Prompts
                .Where(p => p.Id == originalId || p.OriginalPromptId == originalId)
                .OrderByDescending(p => p.Version)
                .ToListAsync();

            return View("Details", prompt);
        }

        private bool PromptExists(int id)
        {
            return _context.Prompts.Any(e => e.Id == id);
        }
    }
} 