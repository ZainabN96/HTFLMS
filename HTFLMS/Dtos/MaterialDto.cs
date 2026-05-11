using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HTFLMS.Dtos
{
    public class MaterialDto
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? ModuleId { get; set; }
        public int? LessonId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = "";

        public IFormFile? File { get; set; }

        public string? FilePath { get; set; }

        public string? ExternalUrl { get; set; }

        public int? Pages { get; set; }
        public int? Slides { get; set; }
        public int? Minutes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}