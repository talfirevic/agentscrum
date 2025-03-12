using System.Diagnostics;
using AgentScrum.Web.Adapters.Contracts.GoogleDocs;
using Microsoft.AspNetCore.Mvc;
using AgentScrum.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace AgentScrum.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<IdentityUser> _signInManager;
    private static readonly Dictionary<string, List<ChatMessage>> _userChatHistory = new();
    private readonly IGoogleDriveAdapter _googleDriveAdapter;

    public HomeController(ILogger<HomeController> logger, SignInManager<IdentityUser> signInManager, IGoogleDriveAdapter googleDriveAdapter)
    {
        _logger = logger;
        _signInManager = signInManager;
        _googleDriveAdapter = googleDriveAdapter;
    }

    public IActionResult Index()
    {
        if (_signInManager.IsSignedIn(User))
        {
            var userId = User.Identity?.Name ?? string.Empty;
            
            if (!_userChatHistory.ContainsKey(userId))
            {
                _userChatHistory[userId] = new List<ChatMessage>
                {
                    new ChatMessage
                    {
                        Content = "Hello! How can I assist you today?",
                        IsUserMessage = false,
                        Timestamp = DateTime.UtcNow
                    }
                };
            }
            
            var viewModel = new ChatViewModel
            {
                Messages = _userChatHistory[userId]
            };
            
            return View("Chat", viewModel);
        } 
        /* var response = _googleDriveAdapter.CreateDocument($"agent-scrum-{Guid.NewGuid().ToString()}", "<h1>Hello!</h1>", "toni@alfirevic.co"); 
        */ 
        return View();
    }

    [HttpPost]
    [Authorize]
    public IActionResult SendMessage(ChatViewModel model)
    {
        var userId = User.Identity?.Name ?? string.Empty;
        
        if (!_userChatHistory.ContainsKey(userId))
        {
            _userChatHistory[userId] = new List<ChatMessage>();
        }
        
        if (!string.IsNullOrWhiteSpace(model.NewMessage))
        {
            // Add user message
            _userChatHistory[userId].Add(new ChatMessage
            {
                Content = model.NewMessage,
                IsUserMessage = true,
                Timestamp = DateTime.UtcNow
            });
            
            // Add AI response (simulated)
            _userChatHistory[userId].Add(new ChatMessage
            {
                Content = GetAIResponse(model.NewMessage),
                IsUserMessage = false,
                Timestamp = DateTime.UtcNow.AddSeconds(1)
            });
        }
        
        model.Messages = _userChatHistory[userId];
        model.NewMessage = string.Empty;
        
        return View("Chat", model);
    }
    
    private string GetAIResponse(string userMessage)
    {
        // Simple AI response simulation
        if (userMessage.Contains("hello", StringComparison.OrdinalIgnoreCase) || 
            userMessage.Contains("hi", StringComparison.OrdinalIgnoreCase))
        {
            return "Hello! How can I help you today?";
        }
        
        if (userMessage.Contains("help", StringComparison.OrdinalIgnoreCase))
        {
            return "I'm here to help! What do you need assistance with?";
        }
        
        if (userMessage.Contains("thank", StringComparison.OrdinalIgnoreCase))
        {
            return "You're welcome! Is there anything else I can help with?";
        }
        
        return "I understand you said: \"" + userMessage + "\". How can I assist you further with that?";
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}