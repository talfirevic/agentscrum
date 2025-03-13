using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AgentScrum.Web.Models
{
    public class Prompt
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [BindNever]
        public int Version { get; set; }
        
        [Required]
        [BindNever]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        [BindNever]
        public string CreatedBy { get; set; } = string.Empty;
        
        // Reference to the original prompt if this is a version
        [BindNever]
        public int? OriginalPromptId { get; set; }
        
        // Flag to indicate if this is the current version
        [BindNever]
        public bool IsCurrentVersion { get; set; }
        
        // Optional category/tag for organizing prompts
        [StringLength(50)]
        public string? Category { get; set; }
    }
} 