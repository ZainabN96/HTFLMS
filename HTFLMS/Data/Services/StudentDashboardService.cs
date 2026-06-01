using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly ApplicationDbContext context;

        public StudentDashboardService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<StudentDashboardDto> GetDashboardAsync(int studentId)
        {
            var student = await context.Users
                .FirstOrDefaultAsync(x => x.Id == studentId && x.MemberType == "Student");

            if (student == null)
            {
                return new StudentDashboardDto();
            }

            var enrollments = await context.CourseEnrollments
                .Include(x => x.Course)
                    .ThenInclude(c => c.Trainer)
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.EnrolledAt)
                .ToListAsync();

            var enrolledCourses = enrollments
                .Where(x => x.Course != null)
                .Select(x => new StudentEnrolledCoursesDto
                {
                    CourseId = x.CourseId,
                    Title = x.Course!.Title,
                    Category = x.Course.Category,
                    TrainerName = x.Course.Trainer != null ? x.Course.Trainer.Name : "No Trainer",
                    CourseImagePath = x.Course.CourseImagePath,
                    BatchStartDate = x.Course.BatchStartDate,
                    BatchEndDate = x.Course.BatchEndDate,
                    EnrollmentStatus = x.Status,
                    ProgressPercentage = 0
                })
                .ToList();
            //not competible with sql  
            //var activeEnrolledCourseIds = enrollments
            //    .Where(x => x.Course != null && x.Status == "Active")
            //    .Select(x => x.CourseId)
            //    .Distinct()
            //    .ToList();

            //var pendingAssignmentQuery = await context.Assignments
            //    .Include(a => a.Course)
            //    .Where(a =>
            //        activeEnrolledCourseIds.Contains(a.CourseId)
            //        && a.IsActive
            //        && !context.AssignmentSubmissions.Any(s =>
            //            s.AssignmentId == a.Id
            //            && s.StudentId == studentId))
            //    .OrderBy(a => a.DueDateTime)
            //    .ToListAsync();
            var pendingAssignmentQuery = await (
    from assignment in context.Assignments
    join enrollment in context.CourseEnrollments
        on assignment.CourseId equals enrollment.CourseId
    where enrollment.StudentId == studentId
          && enrollment.Status == "Active"
          && assignment.IsActive
          && !context.AssignmentSubmissions.Any(submission =>
              submission.AssignmentId == assignment.Id
              && submission.StudentId == studentId)
    orderby assignment.DueDateTime
    select assignment
)
.Include(a => a.Course)
.ToListAsync();
            var now = DateTime.UtcNow;

            var allPendingDeadlines = pendingAssignmentQuery
                .Select(a =>
                {
                    var isOverdue = a.DueDateTime < now;

                    return new StudentUpcomingDeadlineDto
                    {
                        AssignmentId = a.Id,
                        CourseId = a.CourseId,
                        AssignmentTitle = a.Title,
                        CourseTitle = a.Course != null ? a.Course.Title : "Course",
                        DueDateTime = a.DueDateTime,
                        DueText = FormatDueText(a.DueDateTime, now),
                        Status = isOverdue ? "Overdue" : "Pending",
                        StatusClass = isOverdue ? "danger" : "pending",
                        RedirectUrl = $"/Student/Courses/Details/{a.CourseId}?tab=slides-assignments&assignmentId={a.Id}"
                    };
                })
                .ToList();

            var dashboardDeadlines = allPendingDeadlines
                .OrderByDescending(x => x.Status == "Overdue")
                .ThenBy(x => x.DueDateTime)
                .Take(5)
                .ToList();

            return new StudentDashboardDto
            {
                StudentName = student.Name,
                TotalEnrolledCourses = enrollments.Count,
                ActiveCourseCount = enrollments.Count(x => x.Status == "Active"),
                CompletedCourseCount = enrollments.Count(x => x.Status == "Completed"),

                // Abhi trainer-side lesson/grade ka kaam final nahi,
                // is liye safe default rakha hai.
                LessonsCompleted = 0,
                AverageGrade = 0,

                // PendingTasks ab assignments se dynamic hoga.
                PendingTasks = allPendingDeadlines.Count,

                EnrolledCourses = enrolledCourses,
                UpcomingDeadlines = dashboardDeadlines
            };
        }

        private static string FormatDueText(DateTime dueDateTime, DateTime now)
        {
            var dueDate = dueDateTime.Date;
            var today = now.Date;

            if (dueDate < today)
            {
                var overdueDays = (today - dueDate).Days;

                if (overdueDays == 1)
                {
                    return "Yesterday";
                }

                return $"{overdueDays} days overdue";
            }

            if (dueDate == today)
            {
                return $"Today {dueDateTime:hh:mm tt}";
            }

            if (dueDate == today.AddDays(1))
            {
                return $"Tomorrow {dueDateTime:hh:mm tt}";
            }

            var daysLeft = (dueDate - today).Days;

            if (daysLeft <= 7)
            {
                return $"In {daysLeft} days";
            }

            return dueDateTime.ToString("dd MMM, hh:mm tt");
        }
    }
}




//using HTFLMS.Data.IServices;
//using HTFLMS.Dtos;
//using Microsoft.EntityFrameworkCore;

//namespace HTFLMS.Data.Services
//{
//    public class StudentDashboardService : IStudentDashboardService
//    {
//        private readonly ApplicationDbContext context;

//        public StudentDashboardService(ApplicationDbContext context)
//        {
//            this.context = context;
//        }

//        public async Task<StudentDashboardDto> GetDashboardAsync(int studentId)
//        {
//            var student = await context.Users
//                .FirstOrDefaultAsync(x => x.Id == studentId && x.MemberType == "Student");

//            if (student == null)
//            {
//                return new StudentDashboardDto();
//            }

//            var enrollments = await context.CourseEnrollments
//                .Include(x => x.Course)
//                    .ThenInclude(c => c.Trainer)
//                .Where(x => x.StudentId == studentId)
//                .OrderByDescending(x => x.EnrolledAt)
//                .ToListAsync();

//            var enrolledCourses = enrollments
//                .Where(x => x.Course != null)
//                .Select(x => new StudentEnrolledCoursesDto
//                {
//                    CourseId = x.CourseId,
//                    Title = x.Course!.Title,
//                    Category = x.Course.Category,
//                    TrainerName = x.Course.Trainer != null ? x.Course.Trainer.Name : "No Trainer",
//                    CourseImagePath = x.Course.CourseImagePath,
//                    BatchStartDate = x.Course.BatchStartDate,
//                    BatchEndDate = x.Course.BatchEndDate,
//                    EnrollmentStatus = x.Status,
//                    ProgressPercentage = 0
//                })
//                .ToList();

//            return new StudentDashboardDto
//            {
//                StudentName = student.Name,
//                TotalEnrolledCourses = enrollments.Count,
//                ActiveCourseCount = enrollments.Count(x => x.Status == "Active"),
//                CompletedCourseCount = enrollments.Count(x => x.Status == "Completed"),

//                // Abhi trainer-side lesson/grade/assignment ka kaam final nahi,
//                // is liye safe default 0 rakha hai.
//                LessonsCompleted = 0,
//                AverageGrade = 0,
//                PendingTasks = 0,

//                EnrolledCourses = enrolledCourses
//            };
//        }
//    }
//}


