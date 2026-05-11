using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos
{
    public class ModuleDto
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsAccessible { get; set; } = true;

    }
    public class ToggleModuleAccessDto
    {
        public bool IsAccessible { get; set; }
    }
}