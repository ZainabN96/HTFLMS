using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Message { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string Type { get; set; } = "";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
    }
}