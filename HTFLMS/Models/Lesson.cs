using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = "";

        [StringLength(255)]
        public string? VideoUrl { get; set; }

        [StringLength(255)]
        public string? FilePath { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Module? Module { get; set; }
        public ICollection<Material>? Materials { get; set; }
        public ICollection<LessonProgress>? LessonProgresses { get; set; }
    }
}