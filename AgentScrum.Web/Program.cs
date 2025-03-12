using AgentScrum.Web.Adapters.Contracts;
using AgentScrum.Web.Adapters.Contracts.GoogleDocs;
using AgentScrum.Web.Adapters.Contracts.Groq;
using AgentScrum.Web.Adapters.Implementations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AgentScrum.Web.Data;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using System.Text;
using AgentScrum.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
    .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IGroqAdapter>(provider => {
    var configuration = provider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["GroqApiKey"] ?? 
                 throw new InvalidOperationException("Groq API key not found in configuration.");
    return new GroqAdapter(apiKey);
});

builder.Services.AddScoped<IChatGptAdapter, ChatGPTAdapter>();

// Register GoogleDriveAdapter with factory function
builder.Services.AddScoped<IGoogleDriveAdapter>(provider => {
    var dbContext = provider.GetRequiredService<ApplicationDbContext>();
    var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    
    // Create a factory function that creates a DriveService from credentials JSON
    Func<string, DriveService> driveServiceFactory = (credentialsJson) => {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(credentialsJson));
        var credential = GoogleCredential.FromStream(stream)
            .CreateScoped(DriveService.Scope.Drive);
            
        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "AgentScrum API adapter"
        });
    };
    
    // Create the adapter with the factory
    var adapter = new GoogleDriveAdapter(driveServiceFactory);
    
    // Try to get the current user's credentials
    if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
    {
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext.User);
        if (!string.IsNullOrEmpty(userId))
        {
            var credentials = dbContext.GoogleDriveCredentials
                .FirstOrDefault(c => c.UserId == userId && c.IsValid);
                
            if (credentials != null)
            {
                // Set the credentials on the adapter
                adapter.SetCredentials(credentials.CredentialsJson);
            }
        }
    }
    else
    {
        // Fallback to file-based credentials for non-authenticated requests
        try
        {
            if (File.Exists("agentscrum-creds.json"))
            {
                var credentialsJson = File.ReadAllText("agentscrum-creds.json");
                adapter.SetCredentials(credentialsJson);
            }
        }
        catch (Exception ex)
        {
            var logger = provider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Failed to load fallback Google Drive credentials");
        }
    }
    
    return adapter;
});

// Add HttpContextAccessor for accessing the current user
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// Apply database initialization at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Ensure database is created with all tables
        context.Database.EnsureCreated();
        context.Database.Migrate();
        
        // Log information about database initializatio
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

// Use static files
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();