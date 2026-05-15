using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class AssignmentDto
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public int Marks { get; set; }

        [Required]
        public DateTime DueDateTime { get; set; }

        public IFormFile? File { get; set; }

        public bool IsActive { get; set; } = true;
    }
}