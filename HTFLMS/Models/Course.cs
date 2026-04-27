using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [StringLength(255)]
        public string? HandbookFilePath { get; set; }

        [StringLength(255)]
        public string? CourseImagePath { get; set; }

        [Required]
        public int TrainerId { get; set; }

        public bool IsPublished { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime BatchStartDate { get; set; }
        public string? BatchNumber { get; set; }

        public DateTime? BatchEndDate { get; set; }

        public bool CertificateIncluded { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? Trainer { get; set; }
        public ICollection<Module>? Modules { get; set; }
        public ICollection<Material>? Materials { get; set; }
        public ICollection<Assignment>? Assignments { get; set; }
        public ICollection<CourseEnrollment>? Enrollments { get; set; }
        public ICollection<CertificateRequest>? CertificateRequests { get; set; }
    }
}