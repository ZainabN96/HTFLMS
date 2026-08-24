using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class CourseEnrollment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Dropped, Completed

        [Required]
        [StringLength(20)]
        public string DeliveryMode { get; set; } = "Onsite"; // Online, Onsite

        public int? DeliveryModeUpdatedByUserId { get; set; }

        public DateTime? DeliveryModeUpdatedAt { get; set; }

        public DateTime? DroppedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Navigation
        public User? Student { get; set; }
        public Course? Course { get; set; }
        public User? DeliveryModeUpdatedByUser { get; set; }
    }
}