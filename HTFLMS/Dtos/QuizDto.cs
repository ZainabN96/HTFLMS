using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class QuizDto
    {
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        public string? Instructions { get; set; }

        [Required]
        public int AttemptsAllowed { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public bool IsAccessible { get; set; } = false;

        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        [Required]
        public string QuestionText { get; set; } = "";

        [Required]
        public string OptionA { get; set; } = "";

        [Required]
        public string OptionB { get; set; } = "";

        [Required]
        public string OptionC { get; set; } = "";

        [Required]
        public string OptionD { get; set; } = "";

        [Required]
        public string CorrectAnswer { get; set; } = "";

        public int DisplayOrder { get; set; }
    }

    public class ToggleQuizAccessDto
    {
        public bool IsAccessible { get; set; }
    }
}