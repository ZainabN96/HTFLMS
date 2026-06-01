using HTFLMS.Data.IServices;
using HTFLMS.DTOs.StudentGrades;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class StudentGradesService : IStudentGradesService
    {
        private readonly ApplicationDbContext context;

        public StudentGradesService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<GradesPageDto> GetGradesPageAsync(int studentId)
        {
            var response = new GradesPageDto();

            var enrollments = await context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Trainer)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Assignments)
                .Where(e =>
                    e.StudentId == studentId &&
                    (e.Status == "Active" || e.Status == "Completed"))
                .OrderByDescending(e => e.Status == "Active")
                .ThenByDescending(e => e.CompletedAt)
                .ThenByDescending(e => e.EnrolledAt)
                .ToListAsync();

            if (!enrollments.Any())
            {
                response.EmptyMessage = "No grade record found yet. Your grades will appear here once you are enrolled in a course and your submitted work is marked.";
                return response;
            }

            var courseIds = enrollments
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            //var submissions = await context.AssignmentSubmissions
            //    .Include(s => s.Assignment)
            //        .ThenInclude(a => a!.Course)
            //    .Where(s =>
            //        s.StudentId == studentId &&
            //        s.Assignment != null &&
            //        s.Assignment.IsActive &&
            //        courseIds.Contains(s.Assignment.CourseId))
            //    .ToListAsync();
            var submissions = await context.AssignmentSubmissions
    .Include(s => s.Assignment)
        .ThenInclude(a => a!.Course)
    .Where(s =>
        s.StudentId == studentId &&
        s.Assignment != null &&
        s.Assignment.IsActive)
    .ToListAsync();

            submissions = submissions
                .Where(s =>
                    s.Assignment != null &&
                    courseIds.Any(courseId => courseId == s.Assignment.CourseId))
                .ToList();

            var courseDetails = new List<CourseDetailDto>();

            foreach (var enrollment in enrollments)
            {
                if (enrollment.Course == null)
                {
                    continue;
                }

                var course = enrollment.Course;

                var activeAssignments = course.Assignments?
                    .Where(a => a.IsActive)
                    .ToList() ?? new List<Assignment>();

                var courseSubmissions = submissions
                    .Where(s => s.Assignment != null && s.Assignment.CourseId == course.Id)
                    .GroupBy(s => s.AssignmentId)
                    .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
                    .ToList();

                var gradedSubmissions = courseSubmissions
                    .Where(s =>
                        s.IsGraded &&
                        s.ObtainedMarks.HasValue &&
                        s.Assignment != null &&
                        s.Assignment.Marks > 0)
                    .ToList();

                var totalObtainedMarks = gradedSubmissions.Sum(s => s.ObtainedMarks ?? 0);
                var totalMarks = gradedSubmissions.Sum(s => s.Assignment?.Marks ?? 0);

                var averagePercentage = totalMarks > 0
                    ? Math.Round(((decimal)totalObtainedMarks / totalMarks) * 100, 1)
                    : 0;

                var totalAssignments = activeAssignments.Count;
                var submittedAssignments = courseSubmissions.Count;
                var gradedAssignments = gradedSubmissions.Count;
                var pendingReviews = courseSubmissions.Count(s => !s.IsGraded);

                var assignmentProgressPercentage = totalAssignments > 0
                    ? (int)Math.Round(((decimal)gradedAssignments / totalAssignments) * 100)
                    : 0;

                var badge = GetGradeBadge(averagePercentage, gradedAssignments);

                courseDetails.Add(new CourseDetailDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    TrainerName = course.Trainer?.Name ?? "Not assigned",
                    CourseImagePath = course.CourseImagePath,
                    Status = enrollment.Status,
                    TotalAssignments = totalAssignments,
                    SubmittedAssignments = submittedAssignments,
                    GradedAssignments = gradedAssignments,
                    PendingReviews = pendingReviews,
                    AveragePercentage = averagePercentage,
                    AssignmentProgressPercentage = assignmentProgressPercentage,
                    GradeBadgeText = badge.Text,
                    GradeBadgeClass = badge.CssClass
                });
            }

            response.Courses = courseDetails;

            response.RecentResults = submissions
                .Where(s =>
                    s.IsGraded &&
                    s.ObtainedMarks.HasValue &&
                    s.GradedAt.HasValue &&
                    s.Assignment != null &&
                    s.Assignment.Marks > 0)
                .OrderByDescending(s => s.GradedAt)
                .Take(5)
                .Select(s =>
                {
                    var percentage = Math.Round(((decimal)(s.ObtainedMarks ?? 0) / s.Assignment!.Marks) * 100, 1);
                    var badge = GetGradeBadge(percentage, 1);

                    return new RecentResultDto
                    {
                        AssignmentId = s.AssignmentId,
                        AssignmentTitle = s.Assignment.Title,
                        CourseTitle = s.Assignment.Course?.Title ?? "",
                        ObtainedMarks = s.ObtainedMarks ?? 0,
                        TotalMarks = s.Assignment.Marks,
                        Percentage = percentage,
                        GradedAt = s.GradedAt,
                        GradeClass = badge.CssClass
                    };
                })
                .ToList();

            response.Summary = BuildSummary(enrollments, courseDetails, submissions);

            if (!response.Courses.Any())
            {
                response.EmptyMessage = "No grade record found yet. Your grades will appear here once your submitted assignments are marked.";
            }

            return response;
        }

        private GradesCardSummaryDto BuildSummary(
            List<CourseEnrollment> enrollments,
            List<CourseDetailDto> courseDetails,
            List<AssignmentSubmission> submissions)
        {
            var activeCourse = courseDetails
                .FirstOrDefault(c => c.Status == "Active");

            var lastCompletedCourse = courseDetails
                .Where(c => c.Status == "Completed")
                .OrderByDescending(c => c.CourseId)
                .FirstOrDefault();

            var selectedCourse = activeCourse ?? lastCompletedCourse;

            var completedAssignments = submissions
                .GroupBy(s => s.AssignmentId)
                .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
                .Count();

            var pendingReviews = submissions
                .GroupBy(s => s.AssignmentId)
                .Select(g => g.OrderByDescending(x => x.SubmittedAt).First())
                .Count(s => !s.IsGraded);

            var averageTitle = activeCourse != null
                ? "Current Average"
                : lastCompletedCourse != null
                    ? "Last Course Average"
                    : "Current Average";

            var averageMeta = selectedCourse != null
                ? selectedCourse.Status == "Active"
                    ? "Based on your current course."
                    : "Based on your last completed course."
                : "Grades will appear after your work is marked.";

            return new GradesCardSummaryDto
            {
                AverageTitle = averageTitle,
                AveragePercentage = selectedCourse?.AveragePercentage ?? 0,
                AverageMeta = averageMeta,
                CoursesCompleted = enrollments.Count(e => e.Status == "Completed"),
                CompletedAssignments = completedAssignments,
                PendingReviews = pendingReviews
            };
        }

        private static (string Text, string CssClass) GetGradeBadge(decimal percentage, int gradedItems)
        {
            if (gradedItems <= 0)
            {
                return ("No Grade", "fair");
            }

            if (percentage >= 85)
            {
                return ("Excellent", "excellent");
            }

            if (percentage >= 70)
            {
                return ("Good", "good");
            }

            return ("Fair", "fair");
        }
    }
}