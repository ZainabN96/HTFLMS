using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HTFLMS.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        public string? Instructions { get; set; }

        public int Marks { get; set; }

        public DateTime? DueDateUtc { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public string? AttachmentUrl { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = null!;
        public Module? Module { get; set; }

        //public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}