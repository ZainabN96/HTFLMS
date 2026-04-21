using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace HTFLMS.Models
{
    public class Module
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public bool OpenAtStart { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [ValidateNever]
        public Course Course { get; set; } = null!;

        [ValidateNever]
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        [ValidateNever]
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public ICollection<CourseMaterial> Materials { get; set; } = new List<CourseMaterial>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}