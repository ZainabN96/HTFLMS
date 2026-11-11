using System;

namespace HTFLMS.Models
{
    public class QuizAttemptsReset
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int QuizId { get; set; }

        public DateTime LockedUntil { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        // Notification Tracking
        public DateTime? LockNotificationSentAt { get; set; }

        public DateTime? UnlockNotificationSentAt { get; set; }

        // Navigation
        public User? Student { get; set; }

        public Quiz? Quiz { get; set; }
    }
}