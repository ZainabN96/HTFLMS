using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HTFLMS.Models
{
    public class Quiz
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

        public bool IsAccessible { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Module? Module { get; set; }
        public ICollection<QuizQuestion>? Questions { get; set; }
        public ICollection<StudentQuizAttempt>? Attempts { get; set; }
    }
}