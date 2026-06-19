namespace HTFLMS.Dtos.StudentCourseContent
{
    public class StudentCourseContentMaterialsAssignmentsDto
    {
        public int CourseId { get; set; }

        public List<StudentCourseContentMaterialDto> Materials { get; set; } = new();

        public List<StudentCourseContentAssignmentDto> Assignments { get; set; } = new();
    }

    public class StudentCourseContentMaterialDto
    {
        public int Id { get; set; }

        public int? ModuleId { get; set; }

        public string ModuleTitle { get; set; } = "Module not specified";

        public bool IsLocked { get; set; }

        public string Title { get; set; } = "";

        public string ContentType { get; set; } = "";

        public string? FilePath { get; set; }

        public string? ExternalUrl { get; set; }

        public int? Pages { get; set; }

        public int? Slides { get; set; }

        public int? Minutes { get; set; }
    }

    public class StudentCourseContentAssignmentDto
    {
        public int Id { get; set; }

        public int? ModuleId { get; set; }

        public string ModuleTitle { get; set; } = "Module not specified";

        public bool IsLocked { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public int Marks { get; set; }

        public DateTime DueDateTime { get; set; }

        public string? FilePath { get; set; }

        public bool IsSubmitted { get; set; }

        public bool IsGraded { get; set; }

        public int? ObtainedMarks { get; set; }

        public string? Feedback { get; set; }

        public string SubmissionStatus { get; set; } = "Pending Submission";

        public string? SubmittedFilePath { get; set; }

        public string? SubmittedText { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public bool CanSubmit { get; set; }

        public bool IsSubmissionClosed { get; set; }

        public string SubmissionMessage { get; set; } = "";
    }

    public class StudentCourseContentAssignmentSubmitResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";
    }

    public class StudentCourseContentAssignmentUnsubmitResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";
    }
}