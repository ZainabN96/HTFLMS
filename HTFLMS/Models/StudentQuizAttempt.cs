using System;
using System.Collections.Generic;

namespace HTFLMS.Models
{
    public class StudentQuizAttempt
    {
        public int Id { get; set; }

        public int QuizId { get; set; }
        public int StudentId { get; set; }

        public int AttemptNumber { get; set; }
        public int Score { get; set; }
        public bool IsPassed { get; set; } = false;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Quiz? Quiz { get; set; }
        public User? Student { get; set; }
        public ICollection<StudentQuizAttemptAnswer>? Answers { get; set; }
    }
}