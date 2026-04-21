using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Material
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? ModuleId { get; set; }
        public int? LessonId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = "";

        [StringLength(255)]
        public string? FilePath { get; set; }

        [StringLength(255)]
        public string? ExternalUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Course? Course { get; set; }
        public Module? Module { get; set; }
        public Lesson? Lesson { get; set; }
    }
}