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

// Add DriveService registration
builder.Services.AddSingleton<DriveService>(provider => {
    // Configure and create your DriveService instance here
    // This will depend on how you're authenticating with Google APIs
    // Example:
     var credential = GoogleCredential.FromFile("agentscrum-creds.json")
         .CreateScoped(DriveService.Scope.Drive);
     return new DriveService(new BaseClientService.Initializer
     {
    HttpClientInitializer = credential,
         ApplicationName = "AgentScrum API adapter"
     });
});

// Now this will work because DriveService is registered
builder.Services.AddScoped<IGoogleDriveAdapter, GoogleDriveAdapter>();

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
        
        // Ensure database is created (this is safer than Migrate() when no migrations exist)
        context.Database.EnsureCreated();
        
        // Log information about database initialization
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