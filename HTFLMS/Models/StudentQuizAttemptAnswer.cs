namespace HTFLMS.Models
{
    public class StudentQuizAttemptAnswer
    {
        public int Id { get; set; }

        public int AttemptId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }

        public bool IsCorrect { get; set; } = false;

        // Navigation
        public StudentQuizAttempt? Attempt { get; set; }
        public QuizQuestion? Question { get; set; }
        public QuizOption? SelectedOption { get; set; }
    }
}