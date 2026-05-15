using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class LessonDto
    {
        public int Id { get; set; }

        [Required]
        public int ModuleId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        public bool IsActive { get; set; } = true;

        [Required]
        public int DisplayOrder { get; set; }
    }
}