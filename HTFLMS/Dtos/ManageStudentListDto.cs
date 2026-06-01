namespace HTFLMS.Dtos
{
    public class ManageStudentListDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Status { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public int EnrolledCoursesCount { get; set; }

        public double AverageGrade { get; set; } = 0;

        public List<string> EnrolledCourses { get; set; } = new();
    }
}