namespace HTFLMS.Dtos
{
    public class StudentDashboardDto
    {
        public string StudentName { get; set; } = "";

        public int TotalEnrolledCourses { get; set; }

        public int ActiveCourseCount { get; set; }

        public int CompletedCourseCount { get; set; }

        public int LessonsCompleted { get; set; }

        public decimal AverageGrade { get; set; }

        public int PendingTasks { get; set; }

        public List<StudentEnrolledCoursesDto> EnrolledCourses { get; set; } = new();
    }
}