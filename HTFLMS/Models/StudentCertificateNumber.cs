using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class StudentCertificateNumber
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(20)]
        public string DeliveryMode { get; set; } = ""; // Online, Onsite

        [Required]
        public int BaseNumber { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public int? AssignedByUserId { get; set; }

        // Navigation
        public User? Student { get; set; }
        public User? AssignedByUser { get; set; }
        public ICollection<Certificate>? Certificates { get; set; }
    }
}