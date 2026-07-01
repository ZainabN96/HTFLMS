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

        public async Task<GradesTabDto?> GetGradesTabAsync(int studentId, int courseId)
        {
            var enrollment = await context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Trainer)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseId == courseId &&
                    (e.Status == "Active" || e.Status == "Completed"));

            if (enrollment == null || enrollment.Course == null)
            {
                return null;
            }

            var course = enrollment.Course;

            if (!course.IsActive || !course.IsPublished)
            {
                return null;
            }

            var assignments = await context.Assignments
                .Include(a => a.Module)
                .Include(a => a.Submissions!)
                    .ThenInclude(s => s.GradedByUser)
                .Where(a =>
                    a.CourseId == courseId &&
                    a.IsActive)
                .OrderBy(a => a.ModuleId == null ? 0 : 1)
                .ThenBy(a => a.Module != null ? a.Module.DisplayOrder : 0)
                .ThenBy(a => a.DueDateTime)
                .ToListAsync();

            var items = new List<GradesTabItemDto>();

            foreach (var assignment in assignments)
            {
                var latestSubmission = assignment.Submissions?
                    .Where(s => s.StudentId == studentId)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefault();

                items.Add(BuildGradesTabItem(course, assignment, latestSubmission));
            }

            var gradedItems = items
                .Where(i =>
                    i.IsGraded &&
                    i.ObtainedMarks.HasValue &&
                    i.TotalMarks > 0)
                .ToList();

            var totalObtainedMarks = gradedItems.Sum(i => i.ObtainedMarks ?? 0);
            var totalMarks = gradedItems.Sum(i => i.TotalMarks);

            var averagePercentage = totalMarks > 0
                ? Math.Round(((decimal)totalObtainedMarks / totalMarks) * 100, 1)
                : 0;

            var highestItem = gradedItems
                .OrderByDescending(i => i.Percentage ?? 0)
                .FirstOrDefault();

            var badge = GetGradeBadge(averagePercentage, gradedItems.Count);

            return new GradesTabDto
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                Summary = new GradesTabSummaryDto
                {
                    OverallGradeValue = gradedItems.Count > 0
                        ? FormatPercentage(averagePercentage)
                        : "0%",
                    OverallGradeMeta = "Based on graded submissions",

                    GradedItemsValue = $"{gradedItems.Count}/{assignments.Count}",
                    GradedItemsMeta = assignments.Count == 0
                        ? "No assignments added yet"
                        : $"{Math.Max(0, assignments.Count - gradedItems.Count)} items still pending review",

                    HighestScoreValue = highestItem == null
                        ? "N/A"
                        : FormatPercentage(highestItem.Percentage ?? 0),
                    HighestScoreMeta = highestItem == null
                        ? "No graded assignment"
                        : highestItem.AssignmentTitle,

                    CurrentStandingValue = badge.Text,
                    CurrentStandingMeta = GetStandingMeta(badge.Text)
                },
                Items = items,
                EmptyMessage = assignments.Count == 0
                    ? "No assignments found for this course yet."
                    : "No grade record found yet."
            };
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

        private static GradesTabItemDto BuildGradesTabItem(
            Course course,
            Assignment assignment,
            AssignmentSubmission? submission)
        {
            var trainerName = submission?.GradedByUser?.Name
                ?? course.Trainer?.Name
                ?? "Not assigned";

            var moduleTitle = assignment.Module?.Title ?? "Course Level";

            var item = new GradesTabItemDto
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                TypeText = "Assignment",
                ModuleId = assignment.ModuleId,
                ModuleTitle = moduleTitle,
                DueDateTime = assignment.DueDateTime,
                SubmittedAt = submission?.SubmittedAt,
                GradedAt = submission?.GradedAt,
                TrainerName = trainerName,
                TotalMarks = assignment.Marks,
                Feedback = submission?.Feedback ?? "",
                IsSubmitted = submission != null,
                IsGraded = submission != null &&
                           submission.IsGraded &&
                           submission.ObtainedMarks.HasValue &&
                           assignment.Marks > 0
            };

            if (item.IsGraded && submission != null)
            {
                var percentage = Math.Round(((decimal)(submission.ObtainedMarks ?? 0) / assignment.Marks) * 100, 1);
                var result = GetResultText(percentage);

                item.ObtainedMarks = submission.ObtainedMarks;
                item.Percentage = percentage;
                item.CardClass = "graded";
                item.StatusText = "Graded";
                item.StatusClass = "graded";
                item.ScoreText = $"{submission.ObtainedMarks ?? 0}/{assignment.Marks}";
                item.ScorePercentageText = FormatPercentage(percentage);
                item.ResultText = result.Text;
                item.ResultClass = result.CssClass;
                item.SubmissionStatusText = "Submitted Successfully";
                item.ReviewStatusText = "Marked by trainer";

                return item;
            }

            if (submission != null)
            {
                item.CardClass = "pending";
                item.StatusText = "Pending Review";
                item.StatusClass = "pending";
                item.ScoreText = $"--/{assignment.Marks}";
                item.ScorePercentageText = "Pending";
                item.SubmissionStatusText = "Submitted Successfully";
                item.ReviewStatusText = "Waiting for trainer review";
                item.IsPending = true;

                return item;
            }

            var isDuePassed = DateTime.Now > assignment.DueDateTime;

            if (isDuePassed)
            {
                item.CardClass = "missing";
                item.StatusText = "Not Submitted";
                item.StatusClass = "missing";
                item.ScoreText = $"--/{assignment.Marks}";
                item.ScorePercentageText = "Missing";
                item.SubmissionStatusText = "No submission found";
                item.ReviewStatusText = "Submission missing";
                item.IsMissing = true;

                return item;
            }

            item.CardClass = "pending";
            item.StatusText = "Awaiting Submission";
            item.StatusClass = "pending";
            item.ScoreText = $"--/{assignment.Marks}";
            item.ScorePercentageText = "Upcoming";
            item.SubmissionStatusText = "No submission yet";
            item.ReviewStatusText = "Awaiting student submission";
            item.IsAwaitingSubmission = true;

            return item;
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

        private static (string Text, string CssClass) GetResultText(decimal percentage)
        {
            if (percentage >= 85)
            {
                return ("Excellent Work", "good");
            }

            if (percentage >= 70)
            {
                return ("Good Attempt", "average");
            }

            return ("Needs Improvement", "danger");
        }

        private static string GetStandingMeta(string standing)
        {
            return standing switch
            {
                "Excellent" => "Excellent progress in this course",
                "Good" => "Keep completing upcoming work",
                "Fair" => "Review feedback and improve submissions",
                _ => "Grades will appear after your work is marked."
            };
        }

        private static string FormatPercentage(decimal percentage)
        {
            return percentage % 1 == 0
                ? $"{percentage:0}%"
                : $"{percentage:0.0}%";
        }
    }
}