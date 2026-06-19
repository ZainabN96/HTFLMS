namespace HTFLMS.Dtos.TrainerAssignmentGrading
{
    public class TrainerAssignmentGradingListDto
    {
        public TrainerAssignmentGradingSummaryDto Summary { get; set; } = new();
        public TrainerAssignmentGradingFilterOptionsDto Filters { get; set; } = new();
        public List<TrainerAssignmentGradingSubmissionItemDto> Submissions { get; set; } = new();
    }

    public class TrainerAssignmentGradingSummaryDto
    {
        public int TotalSubmissions { get; set; }
        public int Graded { get; set; }
        public int Pending { get; set; }
        public int NotSubmitted { get; set; }
    }

    public class TrainerAssignmentGradingFilterOptionsDto
    {
        public List<TrainerAssignmentGradingCourseOptionDto> Courses { get; set; } = new();
        public List<TrainerAssignmentGradingModuleOptionDto> Modules { get; set; } = new();
        public List<string> Statuses { get; set; } = new()
        {
            "Graded",
            "Pending",
            "Not Submitted"
        };
    }

    public class TrainerAssignmentGradingCourseOptionDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
    }

    public class TrainerAssignmentGradingModuleOptionDto
    {
        public int ModuleId { get; set; }
        public int CourseId { get; set; }
        public string ModuleTitle { get; set; } = "";
    }

    public class TrainerAssignmentGradingSubmissionItemDto
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public int? SubmissionId { get; set; }

        public string StudentName { get; set; } = "";
        public string AssignmentTitle { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public int CourseId { get; set; }

        public string ModuleTitle { get; set; } = "Course Level";
        public int? ModuleId { get; set; }

        public DateTime DueDateTime { get; set; }
        public string DueDateText { get; set; } = "";
        public bool IsDuePassed { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public string SubmittedAtText { get; set; } = "—";

        public bool IsSubmitted { get; set; }
        public bool IsGraded { get; set; }
        public bool IsMarkedZeroMissing { get; set; }

        public string SubmittedFilePath { get; set; } = "";
        public string SubmittedText { get; set; } = "";
        public string Feedback { get; set; } = "";

        public string Status { get; set; } = "";
        public string StatusCssClass { get; set; } = "";

        public int TotalMarks { get; set; }
        public int? ObtainedMarks { get; set; }

        public string ScoreText { get; set; } = "";
        public string ScoreCssClass { get; set; } = "";

        public string ActionText { get; set; } = "—";
        public string ActionUrl { get; set; } = "";
        public bool CanGrade { get; set; }
        public bool CanEdit { get; set; }
        public bool CanMarkZero { get; set; }
    }

    public class TrainerAssignmentGradingDetailDto
    {
        public int SubmissionId { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }

        public string StudentName { get; set; } = "";
        public string AssignmentTitle { get; set; } = "";
        public string CourseTitle { get; set; } = "";
        public string ModuleTitle { get; set; } = "Course Level";

        public DateTime SubmittedAt { get; set; }
        public string SubmittedAtText { get; set; } = "";

        public int TotalMarks { get; set; }
        public int? ObtainedMarks { get; set; }
        public string Feedback { get; set; } = "";

        public bool IsGraded { get; set; }
        public bool IsMarkedZeroMissing { get; set; }

        public string Status { get; set; } = "";
        public string StatusCssClass { get; set; } = "";

        public string CurrentScoreText { get; set; } = "";
        public string CurrentScoreMeta { get; set; } = "";

        public string SubmittedFilePath { get; set; } = "";
        public string SubmittedFileName { get; set; } = "";
        public string SubmittedText { get; set; } = "";

        public string FileExtension { get; set; } = "";
        public string FileViewType { get; set; } = "";
        public bool CanViewFile { get; set; }
        public bool CanDownloadFile { get; set; }
    }

    public class TrainerAssignmentGradingSaveDto
    {
        public int ObtainedMarks { get; set; }
        public string? Feedback { get; set; }
    }

    public class TrainerAssignmentGradingMarkZeroDto
    {
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
    }

    public class TrainerAssignmentGradingSaveResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int SubmissionId { get; set; }
        public int ObtainedMarks { get; set; }
        public int TotalMarks { get; set; }
        public string ScoreText { get; set; } = "";
    }
}