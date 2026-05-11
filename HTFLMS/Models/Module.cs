using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Module
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsAccessible { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Course? Course { get; set; }
        public ICollection<Lesson>? Lessons { get; set; }
        public ICollection<Material>? Materials { get; set; }
        public ICollection<Assignment>? Assignments { get; set; }
        public Quiz? Quiz { get; set; }
        public ICollection<ModuleProgress>? ModuleProgresses { get; set; }
    }
}