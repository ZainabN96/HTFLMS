using Microsoft.AspNetCore.Identity;

namespace HTFLMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Login identifier for students (shown on login page)
        public string UserId { get; set; } = "";
        // Profile section
        public string? Title { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }

        // Enrollment-related
        public string? MemberType { get; set; }   // Student / Learner
        public string? Qualification { get; set; }
        public string? BloodGroup { get; set; }
        public string CNIC { get; set; } = "";

        // Address section
        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }

        // Contact
        public string MobileNumber { get; set; } = "";
        public string? LinkedIn { get; set; }
        public string? EmploymentStatus { get; set; }

        // Profile image saved path
        public string? ProfileImagePath { get; set; }

        // Security question
        public string? SecurityQuestion { get; set; }
        public string? SecurityAnswer { get; set; }
    }
}
