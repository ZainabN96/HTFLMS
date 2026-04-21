using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HTFLMS.Models
{
    public class Enrollment

    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string StudentId { get; set; } = "";

        public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? CompletedAtUtc { get; set; }

        public Course Course { get; set; } = null!;
    }
}