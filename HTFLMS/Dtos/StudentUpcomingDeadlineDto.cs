namespace HTFLMS.Dtos
{
    public class StudentUpcomingDeadlineDto
    {
        public int AssignmentId { get; set; }

        public int CourseId { get; set; }

        public string AssignmentTitle { get; set; } = "";

        public string CourseTitle { get; set; } = "";

        public DateTime DueDateTime { get; set; }

        public string DueText { get; set; } = "";

        public string Status { get; set; } = "";

        public string StatusClass { get; set; } = "";

        public string RedirectUrl { get; set; } = "";
    }
}