namespace HTFLMS.DTOs.StudentGrades
{
    public class CourseDetailDto
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = "";

        public string TrainerName { get; set; } = "Not assigned";

        public string? CourseImagePath { get; set; }

        public string Status { get; set; } = "";

        public int TotalAssignments { get; set; }

        public int SubmittedAssignments { get; set; }

        public int GradedAssignments { get; set; }

        public int PendingReviews { get; set; }

        public decimal AveragePercentage { get; set; }

        public int AssignmentProgressPercentage { get; set; }

        public string GradeBadgeText { get; set; } = "No Grade";

        public string GradeBadgeClass { get; set; } = "fair";
    }
}