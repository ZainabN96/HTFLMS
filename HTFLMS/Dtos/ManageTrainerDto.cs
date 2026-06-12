using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class ManageTrainerDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = "";

        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Designation is required.")]
        public string Designation { get; set; } = "";

        [Required(ErrorMessage = "CNIC is required.")]
        public string CNIC { get; set; } = "";

        [Required(ErrorMessage = "Mobile Number is required.")]
        public string MobileNumber { get; set; } = "";

        [Required(ErrorMessage = "Status is required.")]
        public bool? IsActive { get; set; }

        public string? Gender { get; set; }

        public string? Qualification { get; set; }

        public string? Address { get; set; }

        // NEW
        public IFormFile? Picture { get; set; }

        // NEW
        public string? ProfilePicturePath { get; set; }

        public int AssignedCourseCount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}