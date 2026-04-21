using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public string StudentId { get; set; } = "";

        public string? SubmittedFileUrl { get; set; }

        public DateTime? SubmittedAtUtc { get; set; }

        public decimal? MarksObtained { get; set; }

        public string? Feedback { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Submitted, Graded, Late

        public Assignment Assignment { get; set; } = null!;
    }
}
