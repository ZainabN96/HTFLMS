namespace HTFLMS.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string StudentId { get; set; } = "";

        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedAtUtc { get; set; }

        public Lesson Lesson { get; set; } = null!;
    }
}
