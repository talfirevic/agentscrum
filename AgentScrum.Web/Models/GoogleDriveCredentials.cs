using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AgentScrum.Web.Models;

public class GoogleDriveCredentials
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    public IdentityUser User { get; set; }
    
    [Required]
    public string CredentialsJson { get; set; }
    
    public bool IsValid { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastValidatedAt { get; set; }
} 