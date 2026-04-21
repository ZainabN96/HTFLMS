using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HTFLMS.Models
{
    [Table("Lesson")]

    public class Lesson
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = "";

        public string? Content { get; set; }

        public int DisplayOrder { get; set; }

        public int? DurationMinutes { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Module Module { get; set; } = null!;
    }
}
