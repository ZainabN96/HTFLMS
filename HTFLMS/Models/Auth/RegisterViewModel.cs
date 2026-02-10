using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HTFLMS.Models.Auth
{
    public class RegisterViewModel
    {
        // Login details
        [Required]
        public string UserId { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; } = "";

        // Profile
        public IFormFile? ProfilePicture { get; set; }
        public string? Title { get; set; }

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public DateTime? DateOfBirth { get; set; }

        [Required]
        [RegularExpression("^(Student|Trainer)$", ErrorMessage = "Member type must be Student or Trainer.")]
        public string MemberType { get; set; } = "";

        public string? Qualification { get; set; }
        public string? BloodGroup { get; set; }

        [Required]
        public string CNIC { get; set; } = "";

        // Address
        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }

        // Contact
        [Required]
        public string MobileNumber { get; set; } = "";

        public string? LinkedIn { get; set; }
        public string? EmploymentStatus { get; set; }

        // Security
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
    }
}
