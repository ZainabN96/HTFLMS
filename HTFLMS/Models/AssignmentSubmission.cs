using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [StringLength(255)]
        public string? SubmittedFilePath { get; set; }

        public string? SubmittedText { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public int? ObtainedMarks { get; set; }

        public string? Feedback { get; set; }

        public bool IsGraded { get; set; } = false;

        public int? GradedByUserId { get; set; }

        public DateTime? GradedAt { get; set; }

        // Navigation
        public Assignment? Assignment { get; set; }
        public User? Student { get; set; }
        public User? GradedByUser { get; set; }
    }
}