using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        [Required]
        public int CertificateRequestId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? StudentCertificateNumberId { get; set; }

        [Required]
        [StringLength(20)]
        public string DeliveryMode { get; set; } = ""; // Online, Onsite

        [Required]
        public int CertificateYear { get; set; }

        [Required]
        public int BaseNumber { get; set; }

        [StringLength(5)]
        public string? Suffix { get; set; } // A, B, C...

        [Required]
        [StringLength(50)]
        public string CertificateId { get; set; } = ""; // HTF-2026-0001 / HTF-2026-0001-A

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(255)]
        public string CertificateFilePath { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string StudentNameSnapshot { get; set; } = "";

        [StringLength(10)]
        public string? TitlePrefixSnapshot { get; set; }

        [Required]
        [StringLength(200)]
        public string CourseTitleSnapshot { get; set; } = "";

        [StringLength(100)]
        public string? BatchNumberSnapshot { get; set; }

        [StringLength(100)]
        public string? DurationSnapshot { get; set; }

        public DateTime BatchStartDateSnapshot { get; set; }

        public DateTime? BatchEndDateSnapshot { get; set; }

        [Required]
        [StringLength(20)]
        public string DeliveryModeSnapshot { get; set; } = "";

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public int? GeneratedByUserId { get; set; }

        // Navigation
        public CertificateRequest? CertificateRequest { get; set; }
        public User? Student { get; set; }
        public Course? Course { get; set; }
        public StudentCertificateNumber? StudentCertificateNumber { get; set; }
        public User? GeneratedByUser { get; set; }
    }
}