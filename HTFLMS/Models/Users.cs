using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        // Title -> Gender
        [Required]
        public string Gender { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        // optional
        public string? Qualification { get; set; }

        public string MemberType { get; set; } = "Student";

        [Required]
        public string CNIC { get; set; } = "";

        // Address optional, Country/City required
        public string? Address { get; set; }

        [Required]
        public string Country { get; set; } = "";

        [Required]
        public string City { get; set; } = "";

        [Required]
        public string MobileNumber { get; set; } = "";

        public string? LinkedIn { get; set; }

        [Required]
        public string EmploymentStatus { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string Designation { get; set; } = "";
    }
}
