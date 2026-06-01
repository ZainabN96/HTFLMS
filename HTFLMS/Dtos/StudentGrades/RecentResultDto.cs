namespace HTFLMS.DTOs.StudentGrades
{
    public class RecentResultDto
    {
        public int AssignmentId { get; set; }

        public string AssignmentTitle { get; set; } = "";

        public string CourseTitle { get; set; } = "";

        public int ObtainedMarks { get; set; }

        public int TotalMarks { get; set; }

        public decimal Percentage { get; set; }

        public DateTime? GradedAt { get; set; }

        public string GradeClass { get; set; } = "fair";

        public string MarkText => $"{ObtainedMarks}/{TotalMarks}";
    }
}