namespace HTFLMS.Dtos.StudentCourseContent
{
    public class StudentCourseContentInfoDto
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = "";

        public string Category { get; set; } = "";

        public string Description { get; set; } = "";

        public string? CourseImagePath { get; set; }

        public string TrainerName { get; set; } = "No Trainer";

        public bool CertificateIncluded { get; set; }

        public DateTime BatchStartDate { get; set; }

        public DateTime? BatchEndDate { get; set; }

        public string? DurationText { get; set; }

        public int TotalModules { get; set; }

        public int TotalLessons { get; set; }
    }
}