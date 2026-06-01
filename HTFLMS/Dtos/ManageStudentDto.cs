using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class ManageStudentDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        public string? Password { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        public DateTime? JoinDate { get; set; }

        public List<int> CourseIds { get; set; } = new();
    }
}