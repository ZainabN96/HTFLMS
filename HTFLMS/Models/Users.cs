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

        public string? Gender { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        // optional
        public string? Qualification { get; set; }

        public string MemberType { get; set; } = "Student";

        [Required]
        public string CNIC { get; set; } = "";

        // Address optional, Country/City required
        public string? Address { get; set; }

       
        public string? Country { get; set; } = "";

      
        public string? City { get; set; } = "";

        [Required]
        public string MobileNumber { get; set; } = "";

        public string? LinkedIn { get; set; }

        
        public string? EmploymentStatus { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Designation { get; set; } = "";

        public string? ProfilePicturePath { get; set; }
        // Navigation Properties
        public ICollection<Course>? TrainerCourses { get; set; }
        public ICollection<CourseEnrollment>? Enrollments { get; set; }
        public ICollection<AssignmentSubmission>? AssignmentSubmissions { get; set; }
        public ICollection<LessonProgress>? LessonProgresses { get; set; }
        public ICollection<ModuleProgress>? ModuleProgresses { get; set; }
        public ICollection<StudentQuizAttempt>? StudentQuizAttempts { get; set; }
        public ICollection<Notification>? Notifications { get; set; }
        public ICollection<CertificateRequest>? RequestedCertificates { get; set; }
        public ICollection<CertificateRequest>? ApprovedCertificates { get; set; }
    }
}
   
