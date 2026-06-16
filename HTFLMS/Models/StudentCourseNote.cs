using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class StudentCourseNote
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User? Student { get; set; }

        public Course? Course { get; set; }
    }
}