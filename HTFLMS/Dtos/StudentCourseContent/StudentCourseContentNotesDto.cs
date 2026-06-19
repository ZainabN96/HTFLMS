using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Dtos.StudentCourseContent
{
    public class StudentCourseContentNotesDto
    {
        public int CourseId { get; set; }

        public int TotalNotes { get; set; }

        public string RecentActivityText { get; set; } = "No activity yet";

        public string LastUpdatedText { get; set; } = "No notes yet";

        public List<StudentCourseContentNoteDto> Notes { get; set; } = new();
    }

    public class StudentCourseContentNoteDto
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class StudentCourseContentNoteSaveDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";
    }

    public class StudentCourseContentNoteActionResultDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = "";

        public StudentCourseContentNoteDto? Note { get; set; }
    }
}