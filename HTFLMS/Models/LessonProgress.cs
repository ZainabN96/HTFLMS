using System;

namespace HTFLMS.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int LessonId { get; set; }

        public bool IsViewed { get; set; } = false;
        public DateTime? ViewedAt { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public User? Student { get; set; }
        public Lesson? Lesson { get; set; }
    }
}