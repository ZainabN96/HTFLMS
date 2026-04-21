using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HTFLMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";
        public int TrainerId { get; set; }
        public User Trainer { get; set; } = null!;
        [Required, MaxLength(80)]
        public string Category { get; set; } = "";

        [Required, MaxLength(120)]
        public string InstructorName { get; set; } = "";

        [MaxLength(80)]
        public string? Level { get; set; }

        [MaxLength(120)]
        public string? CourseCode { get; set; }

        [MaxLength(80)]
        public string? Duration { get; set; }

        public int? TotalModules { get; set; }

        [MaxLength(80)]
        public string? Language { get; set; }

        [MaxLength(50)]
        public string? CertificateOption { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [MaxLength(500)]
        public string? ShortDescription { get; set; }

        public string? Content { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation
        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public ICollection<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}