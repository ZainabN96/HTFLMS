using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public int Marks { get; set; }

        [Required]
        public DateTime DueDateTime { get; set; }

        [StringLength(255)]
        public string? FilePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Course? Course { get; set; }
        public Module? Module { get; set; }
        public ICollection<AssignmentSubmission>? Submissions { get; set; }
    }
}