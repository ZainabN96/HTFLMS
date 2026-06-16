using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos.StudentCourseContent
{
    public class StudentCourseContentQuizDto
    {
        public int QuizId { get; set; }

        public int ModuleId { get; set; }

        public string Title { get; set; } = "";

        public string? Instructions { get; set; }

        public int AttemptsAllowed { get; set; } = 3;

        public int AttemptsUsed { get; set; }

        public int AttemptsLeft { get; set; }

        public bool IsPassed { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockedUntil { get; set; }

        public string Message { get; set; } = "";

        public List<StudentCourseContentQuizQuestionDto> Questions { get; set; } = new();
    }

    public class StudentCourseContentQuizQuestionDto
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = "";

        public int DisplayOrder { get; set; }

        public List<StudentCourseContentQuizOptionDto> Options { get; set; } = new();
    }

    public class StudentCourseContentQuizOptionDto
    {
        public int OptionId { get; set; }

        public string OptionText { get; set; } = "";
    }

    public class StudentCourseContentQuizSubmitDto
    {
        [Required]
        public int QuizId { get; set; }

        public List<StudentCourseContentQuizSubmitAnswerDto> Answers { get; set; } = new();
    }

    public class StudentCourseContentQuizSubmitAnswerDto
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int SelectedOptionId { get; set; }
    }

    public class StudentCourseContentQuizResultDto
    {
        public bool Success { get; set; }

        public bool IsPassed { get; set; }

        public bool IsLocked { get; set; }

        public DateTime? LockedUntil { get; set; }

        public int Score { get; set; }

        public int TotalQuestions { get; set; }

        public int CorrectAnswers { get; set; }

        public decimal Percentage { get; set; }

        public int AttemptsUsed { get; set; }

        public int AttemptsLeft { get; set; }

        public string Message { get; set; } = "";
    }
    public class StudentCourseContentQuizReviewDto
    {
        public int QuizId { get; set; }

        public string QuizTitle { get; set; } = "";

        public int AttemptId { get; set; }

        public int AttemptNumber { get; set; }

        public bool IsPassed { get; set; }

        public int Score { get; set; }

        public DateTime SubmittedAt { get; set; }

        public string Message { get; set; } = "";
        public int AttemptsUsed { get; set; }

        public int AttemptsLeft { get; set; }

        public bool IsLocked { get; set; }

        public List<StudentCourseContentQuizReviewQuestionDto> Questions { get; set; } = new();
    }

    public class StudentCourseContentQuizReviewQuestionDto
    {
        public int QuestionId { get; set; }

        public string QuestionText { get; set; } = "";

        public List<StudentCourseContentQuizReviewOptionDto> Options { get; set; } = new();
    }

    public class StudentCourseContentQuizReviewOptionDto
    {
        public int OptionId { get; set; }

        public string OptionText { get; set; } = "";

        public bool IsSelected { get; set; }

        public bool IsSelectedCorrect { get; set; }

        public bool ShowAsCorrectAnswer { get; set; }
    }
}