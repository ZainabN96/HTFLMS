using System;

namespace HTFLMS.Models
{
    public class ModuleProgress
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int ModuleId { get; set; }

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public User? Student { get; set; }
        public Module? Module { get; set; }
    }
}