namespace HTFLMS.Models
{
    public class AssignmentDueReminder
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public int StudentId { get; set; }

        public bool NotificationSent { get; set; }

        public bool EmailSent { get; set; }

        public DateTime? NotificationSentAt { get; set; }

        public DateTime? EmailSentAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Assignment? Assignment { get; set; }

        public User? Student { get; set; }
    }
}