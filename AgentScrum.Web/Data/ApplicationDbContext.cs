using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AgentScrum.Web.Models;

namespace AgentScrum.Web.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<GoogleDriveCredentials> GoogleDriveCredentials { get; set; }
    public DbSet<Prompt> Prompts { get; set; }
}