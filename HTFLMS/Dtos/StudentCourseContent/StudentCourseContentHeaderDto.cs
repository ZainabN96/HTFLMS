namespace HTFLMS.Dtos.StudentCourseContent
{
    public class StudentCourseContentHeaderDto
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = "";

        public string? CourseImagePath { get; set; }

        public string TrainerName { get; set; } = "No Trainer";

        public bool CertificateIncluded { get; set; }

        public int ProgressPercentage { get; set; }
    }
}