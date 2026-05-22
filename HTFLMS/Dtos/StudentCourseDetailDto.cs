namespace HTFLMS.Dtos
{
    public class StudentCourseDetailDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Category { get; set; } = "";

        public string Description { get; set; } = "";

        public string? HandbookFilePath { get; set; }

        public string? CourseImagePath { get; set; }

        public string? BatchNumber { get; set; }

        public string? DurationText { get; set; }

        public DateTime BatchStartDate { get; set; }

        public DateTime? BatchEndDate { get; set; }

        public bool CertificateIncluded { get; set; }

        public string TrainerName { get; set; } = "";
    }
}