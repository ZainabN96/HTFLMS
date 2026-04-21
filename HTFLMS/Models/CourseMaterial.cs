using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HTFLMS.Models
{
    public class CourseMaterial
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? ModuleId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        [Required, MaxLength(50)]
        public string ContentType { get; set; } = ""; // PDF, PPTX, VideoLink, Doc

        public string? FileUrl { get; set; }
        public string? ExternalLink { get; set; }

        [MaxLength(255)]
        public string? FileName { get; set; }

        public long? FileSizeBytes { get; set; }

        [MaxLength(100)]
        public string? PagesOrLength { get; set; } // 12 Pages, 18 Slides, 22 Min

        public int SortOrder { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = null!;
        public Module? Module { get; set; }
    }
}