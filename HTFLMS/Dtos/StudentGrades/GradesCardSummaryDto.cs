namespace HTFLMS.DTOs.StudentGrades
{
    public class GradesCardSummaryDto
    {
        public string AverageTitle { get; set; } = "Current Average";

        public decimal AveragePercentage { get; set; }

        public string AverageMeta { get; set; } = "Grades will appear after your work is marked.";

        public int CoursesCompleted { get; set; }

        public int CompletedAssignments { get; set; }

        public int PendingReviews { get; set; }
    }
}