using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTFLMS.Models
{
    [Table("QuizQuestion")]
    public class QuizQuestion
    {
        public int Id { get; set; }
        public int QuizId { get; set; }

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

        [Required, MaxLength(20)]
        public string CorrectAnswer { get; set; } = ""; // OptionA / OptionB etc.

        public int DisplayOrder { get; set; }

        public Quiz Quiz { get; set; } = null!;
    }
}
