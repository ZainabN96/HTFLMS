namespace HTFLMS.Dtos.CertificateReview
{
    public class CertificateReviewListDto
    {
        public CertificateReviewSummaryDto Summary { get; set; } = new();
        public CertificateReviewFilterDto Filters { get; set; } = new();

        public int SelectedCourseId { get; set; }
        public string SelectedCourseTitle { get; set; } = "";
        public bool IsCourseEnded { get; set; }

        public List<CertificateReviewAssignmentColumnDto> Assignments { get; set; } = new();
        public List<CertificateReviewStudentRowDto> Students { get; set; } = new();

        public string EmptyMessage { get; set; } = "No students found for this course.";
    }

    public class CertificateReviewSummaryDto
    {
        public int TotalStudents { get; set; }

        public decimal OverallClassAverage { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public int AtRiskStudents { get; set; }

        public int InProgress { get; set; }
        public int NotApplied { get; set; }
        public int PendingRequests { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
    }

    public class CertificateReviewFilterDto
    {
        public List<CertificateReviewCourseOptionDto> Courses { get; set; } = new();

        public List<string> CertificateStatuses { get; set; } = new()
        {
            "All",
            "In Progress",
            "Not Applied",
            "Pending",
            "Approved",
            "Rejected"
        };
    }

    public class CertificateReviewCourseOptionDto
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";
    }

    public class CertificateReviewAssignmentColumnDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = "";
        public string ShortTitle { get; set; } = "";
        public int TotalMarks { get; set; }
        public DateTime DueDateTime { get; set; }
        public string DueDateText { get; set; } = "";
    }

    public class CertificateReviewStudentRowDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";

        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = "";

        public List<CertificateReviewAssignmentCellDto> AssignmentCells { get; set; } = new();

        public int TotalMarks { get; set; }
        public int ObtainedMarks { get; set; }

        public decimal OverallPercentage { get; set; }
        public string OverallText { get; set; } = "0%";
        public string OverallCssClass { get; set; } = "";

        public string StandingText { get; set; } = "";
        public string StandingCssClass { get; set; } = "";

        public int? CertificateRequestId { get; set; }
        public string CertificateStatus { get; set; } = "";
        public string CertificateStatusText { get; set; } = "";
        public string CertificateStatusCssClass { get; set; } = "";

        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
    }

    public class CertificateReviewAssignmentCellDto
    {
        public int AssignmentId { get; set; }
        public string AssignmentTitle { get; set; } = "";

        public int TotalMarks { get; set; }
        public int? ObtainedMarks { get; set; }

        public string ValueText { get; set; } = "";
        public string ValueCssClass { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusCssClass { get; set; } = "";

        public bool IsScore { get; set; }
        public bool IsSubmitted { get; set; }
        public bool IsGraded { get; set; }
        public bool IsMissing { get; set; }
        public bool IsPending { get; set; }
    }

    public class CertificateReviewActionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}