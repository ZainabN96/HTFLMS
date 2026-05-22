namespace HTFLMS.Dtos
{
    public class StudentEnrolledCoursesDto
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = "";

        public string Category { get; set; } = "";

        public string? TrainerName { get; set; }

        public string? CourseImagePath { get; set; }

        public DateTime BatchStartDate { get; set; }

        public DateTime? BatchEndDate { get; set; }

        public string EnrollmentStatus { get; set; } = "";

        public int ProgressPercentage { get; set; } = 0;
    }
}