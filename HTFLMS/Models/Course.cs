using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        [Required, MaxLength(80)]
        public string Category { get; set; } = "";

        [Required, MaxLength(120)]
        public string InstructorName { get; set; } = "";

        [MaxLength(80)]
        public string? Level { get; set; }

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        public string? Content { get; set; }

        public string? ImageUrl { get; set; }

        [MaxLength(50)]
        public string? CourseCode { get; set; }

        [MaxLength(80)]
        public string? Duration { get; set; }

        [MaxLength(80)]
        public string? Language { get; set; }

        public int? TotalModules { get; set; }

        [MaxLength(50)]
        public string? Certificate { get; set; }

        /* REQUIRED STATUS */
        [Required]
        public string Status { get; set; } = "";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public string TrainerId { get; set; } = "";
    }
}