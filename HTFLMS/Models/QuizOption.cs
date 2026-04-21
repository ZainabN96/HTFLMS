using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class QuizOption
    {
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(255)]
        public string OptionText { get; set; } = "";

        public bool IsCorrect { get; set; } = false;

        // Navigation
        public QuizQuestion? Question { get; set; }
    }
}