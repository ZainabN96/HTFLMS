using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class CertificateRequest
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Uploaded

        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }

        [StringLength(255)]
        public string? CertificateFilePath { get; set; }

        public DateTime? UploadedAt { get; set; }

        // Navigation
        public User? Student { get; set; }
        public Course? Course { get; set; }
        public User? ApprovedByUser { get; set; }
    }
}