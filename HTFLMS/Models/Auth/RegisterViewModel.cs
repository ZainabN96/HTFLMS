using System;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models.Auth
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other.")]
        public string Gender { get; set; } = "";

        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        public DateTime? DateOfBirth { get; set; }   // optional
        public string? Qualification { get; set; }  // optional

        [Required]
        [RegularExpression("^(Student|Trainer)$", ErrorMessage = "Member type must be Student or Trainer.")]
        public string MemberType { get; set; } = "";

        [Required]
        public string CNIC { get; set; } = "";

        public string? Address { get; set; } // optional

        [Required]
        public string Country { get; set; } = "";

        [Required]
        public string City { get; set; } = "";

        [Required]
        public string MobileNumber { get; set; } = "";

        public string? LinkedIn { get; set; }

        [Required]
        [RegularExpression("^(Searching for Job|Employed|Student)$", ErrorMessage = "Select a valid employment status.")]
        public string EmploymentStatus { get; set; } = "";

        // 🔥 Added back
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
