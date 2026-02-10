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

        public string? Title { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string MemberType { get; set; } = "Student";

        public string? Qualification { get; set; }
        public string? BloodGroup { get; set; }

        [Required]
        public string CNIC { get; set; } = "";

        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }

        [Required]
        public string MobileNumber { get; set; } = "";

        public string? LinkedIn { get; set; }
        public string? EmploymentStatus { get; set; }

        public string? ProfileImagePath { get; set; }

        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
