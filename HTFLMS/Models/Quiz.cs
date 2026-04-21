using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTFLMS.Models
{
    [Table("Quiz")]
    public class Quiz
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        public string? Instructions { get; set; }

        public int AttemptsAllowed { get; set; } = 1;

        public int DisplayOrder { get; set; }

        public bool IsAccessible { get; set; } = false;

        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Module Module { get; set; } = null!;
        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
