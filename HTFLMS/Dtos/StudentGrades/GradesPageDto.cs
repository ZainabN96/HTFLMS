namespace HTFLMS.DTOs.StudentGrades
{
    public class GradesPageDto
    {
        public GradesCardSummaryDto Summary { get; set; } = new GradesCardSummaryDto();

        public List<CourseDetailDto> Courses { get; set; } = new List<CourseDetailDto>();

        public List<RecentResultDto> RecentResults { get; set; } = new List<RecentResultDto>();

        public string EmptyMessage { get; set; } = "No grade record found yet.";

        public bool HasData => Courses.Any();
    }
}