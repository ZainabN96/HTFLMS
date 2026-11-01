using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = "";
        public string? ProfilePicturePath { get; set; }
        public string? Password { get; set; }
        public string? PasswordHash { get; set; } = "";

        // Title -> Gender
        [Required]
        public string Gender { get; set; } = "";
        [Required(ErrorMessage = "Title prefix is required.")]
        [RegularExpression(@"^(Ms\.|Mr\.|Mrs\.)$",
    ErrorMessage = "Please select a valid title prefix.")]
        public string TitlePrefix { get; set; } = "";
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

        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
