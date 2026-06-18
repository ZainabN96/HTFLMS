namespace HTFLMS.Dtos.StudentCourseContent
{
    public class StudentCourseContentModulesDto
    {
        public int CourseId { get; set; }

        public int ProgressPercentage { get; set; }

        public List<StudentCourseContentModuleDto> Modules { get; set; } = new();
    }

    public class StudentCourseContentModuleDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public int DisplayOrder { get; set; }

        public bool IsAccessible { get; set; }

        public bool IsCompleted { get; set; }

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public int ProgressPercentage { get; set; }

        public string StatusText { get; set; } = "Not Started";

        public List<StudentCourseContentLessonDto> Lessons { get; set; } = new();

        public List<StudentCourseContentModuleQuizDto> Quizzes { get; set; } = new();
    }

    public class StudentCourseContentLessonDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public int DisplayOrder { get; set; }

        public bool IsCompleted { get; set; }
    }

    public class StudentCourseContentModuleQuizDto
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }

        public string Title { get; set; } = "";

        public string? Instructions { get; set; }

        public int AttemptsAllowed { get; set; } = 3;

        public int AttemptsUsed { get; set; }

        public int AttemptsLeft { get; set; }

        public int QuestionsCount { get; set; }

        public int? LastScorePercentage { get; set; }

        public bool IsPassed { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockedUntil { get; set; }

        public bool CanViewAttempt { get; set; }

        public bool CanRetake { get; set; }

        public string StatusText { get; set; } = "Start Quiz";
    }
}