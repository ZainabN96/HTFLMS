using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string QuestionText { get; set; } = "";

        [Required]
        public int DisplayOrder { get; set; }

        // Navigation
        public Quiz? Quiz { get; set; }
        public ICollection<QuizOption>? Options { get; set; }
        public ICollection<StudentQuizAttemptAnswer>? AttemptAnswers { get; set; }
    }
}