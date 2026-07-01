namespace HTFLMS.DTOs.StudentGrades
{
    public class GradesTabDto
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = "";

        public GradesTabSummaryDto Summary { get; set; } = new GradesTabSummaryDto();

        public List<GradesTabItemDto> Items { get; set; } = new List<GradesTabItemDto>();

        public string EmptyMessage { get; set; } = "No grade record found yet.";

        public bool HasData => Items.Any();
    }

    public class GradesTabSummaryDto
    {
        public string OverallGradeValue { get; set; } = "0%";

        public string OverallGradeMeta { get; set; } = "Based on graded submissions";

        public string GradedItemsValue { get; set; } = "0/0";

        public string GradedItemsMeta { get; set; } = "No graded items yet";

        public string HighestScoreValue { get; set; } = "N/A";

        public string HighestScoreMeta { get; set; } = "No graded assignment";

        public string CurrentStandingValue { get; set; } = "No Grade";

        public string CurrentStandingMeta { get; set; } = "Grades will appear after your work is marked.";
    }

    public class GradesTabItemDto
    {
        public int AssignmentId { get; set; }

        public string AssignmentTitle { get; set; } = "";

        public string TypeText { get; set; } = "Assignment";

        public int? ModuleId { get; set; }

        public string ModuleTitle { get; set; } = "Course Level";

        public DateTime DueDateTime { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public DateTime? GradedAt { get; set; }

        public string TrainerName { get; set; } = "Not assigned";

        public int TotalMarks { get; set; }

        public int? ObtainedMarks { get; set; }

        public decimal? Percentage { get; set; }

        public string CardClass { get; set; } = "missing";

        public string StatusText { get; set; } = "Not Submitted";

        public string StatusClass { get; set; } = "missing";

        public string ScoreText { get; set; } = "--/0";

        public string ScorePercentageText { get; set; } = "Missing";

        public string ResultText { get; set; } = "";

        public string ResultClass { get; set; } = "";

        public string Feedback { get; set; } = "";

        public string SubmissionStatusText { get; set; } = "";

        public string ReviewStatusText { get; set; } = "";

        public bool IsSubmitted { get; set; }

        public bool IsGraded { get; set; }

        public bool IsMissing { get; set; }

        public bool IsPending { get; set; }

        public bool IsAwaitingSubmission { get; set; }
    }
}