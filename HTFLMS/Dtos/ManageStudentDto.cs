using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class ManageStudentDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title prefix is required.")]
        [RegularExpression("^(Mr\\.|Ms\\.)$", ErrorMessage = "Please select a valid title prefix.")]
        public string TitlePrefix { get; set; } = "";

        [Required(ErrorMessage = "Student name is required.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = "";

        public string? Password { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = "Active";

        public DateTime? JoinDate { get; set; }

        public List<int> CourseIds { get; set; } = new();
    }
}