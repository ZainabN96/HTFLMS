using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class CourseDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public int TrainerId { get; set; }

        [Required]
        public DateTime BatchStartDate { get; set; }

        public DateTime? BatchEndDate { get; set; }

        public string? BatchNumber { get; set; }

        public bool CertificateIncluded { get; set; }

        public string? Status { get; set; }

        public IFormFile? ImageFile { get; set; }

        public IFormFile? HandbookFile { get; set; }
    }
}