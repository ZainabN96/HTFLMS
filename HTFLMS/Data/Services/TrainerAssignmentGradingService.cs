using HTFLMS.Data.IServices;
using HTFLMS.Dtos.TrainerAssignmentGrading;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class TrainerAssignmentGradingService : ITrainerAssignmentGradingService
    {
        private const string MissingZeroFeedback = "No submission received before the due date.";

        private readonly ApplicationDbContext context;

        public TrainerAssignmentGradingService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<TrainerAssignmentGradingListDto> GetSubmissionsAsync(
            int trainerId,
            string? search,
            int? courseId,
            int? moduleId,
            string? status)
        {
            var query =
                from assignment in context.Assignments
                join course in context.Courses
                    on assignment.CourseId equals course.Id
                join enrollment in context.CourseEnrollments
                    on course.Id equals enrollment.CourseId
                join student in context.Users
                    on enrollment.StudentId equals student.Id
                join module in context.Modules
                    on assignment.ModuleId equals (int?)module.Id into moduleGroup
                from module in moduleGroup.DefaultIfEmpty()
                join submission in context.AssignmentSubmissions
                    on new
                    {
                        AssignmentId = assignment.Id,
                        StudentId = enrollment.StudentId
                    }
                    equals new
                    {
                        AssignmentId = submission.AssignmentId,
                        StudentId = submission.StudentId
                    }
                    into submissionGroup
                from submission in submissionGroup.DefaultIfEmpty()
                where course.TrainerId == trainerId
                      && course.IsActive
                      && assignment.IsActive
                      && enrollment.Status == "Active"
                      && student.IsActive
                select new TrainerAssignmentGradingSubmissionItemDto
                {
                    AssignmentId = assignment.Id,
                    StudentId = student.Id,
                    SubmissionId = submission == null ? null : submission.Id,

                    StudentName = student.Name,
                    AssignmentTitle = assignment.Title,
                    CourseTitle = course.Title,
                    CourseId = course.Id,

                    ModuleId = assignment.ModuleId,
                    ModuleTitle = module == null ? "Course Level" : module.Title,

                    DueDateTime = assignment.DueDateTime,

                    SubmittedAt = submission == null ? null : submission.SubmittedAt,
                    IsSubmitted = submission != null,
                    IsGraded = submission != null && submission.IsGraded,

                    SubmittedFilePath = submission == null ? "" : (submission.SubmittedFilePath ?? ""),
                    SubmittedText = submission == null ? "" : (submission.SubmittedText ?? ""),
                    Feedback = submission == null ? "" : (submission.Feedback ?? ""),

                    TotalMarks = assignment.Marks,
                    ObtainedMarks = submission == null ? null : submission.ObtainedMarks
                };

            if (courseId.HasValue && courseId.Value > 0)
            {
                query = query.Where(x => x.CourseId == courseId.Value);
            }

            if (moduleId.HasValue && moduleId.Value > 0)
            {
                query = query.Where(x => x.ModuleId == moduleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.StudentName.ToLower().Contains(keyword) ||
                    x.AssignmentTitle.ToLower().Contains(keyword) ||
                    x.CourseTitle.ToLower().Contains(keyword) ||
                    x.ModuleTitle.ToLower().Contains(keyword));
            }

            var submissions = await query
                .OrderByDescending(x => x.SubmittedAt.HasValue)
                .ThenByDescending(x => x.SubmittedAt)
                .ThenBy(x => x.StudentName)
                .ThenBy(x => x.AssignmentTitle)
                .ToListAsync();

            foreach (var item in submissions)
            {
                ApplyListComputedFields(item);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                submissions = submissions
                    .Where(x => string.Equals(
                        x.Status,
                        status.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return new TrainerAssignmentGradingListDto
            {
                Summary = new TrainerAssignmentGradingSummaryDto
                {
                    TotalSubmissions = submissions.Count,
                    Graded = submissions.Count(x => x.Status == "Graded"),
                    Pending = submissions.Count(x => x.Status == "Pending"),
                    NotSubmitted = submissions.Count(x => x.Status == "Not Submitted")
                },
                Filters = await GetFilterOptionsAsync(trainerId),
                Submissions = submissions
            };
        }

        public async Task<TrainerAssignmentGradingDetailDto?> GetSubmissionDetailAsync(
            int trainerId,
            int submissionId)
        {
            var detail =
                await (
                    from submission in context.AssignmentSubmissions
                    join assignment in context.Assignments
                        on submission.AssignmentId equals assignment.Id
                    join course in context.Courses
                        on assignment.CourseId equals course.Id
                    join enrollment in context.CourseEnrollments
                        on new
                        {
                            StudentId = submission.StudentId,
                            CourseId = assignment.CourseId
                        }
                        equals new
                        {
                            StudentId = enrollment.StudentId,
                            CourseId = enrollment.CourseId
                        }
                    join student in context.Users
                        on submission.StudentId equals student.Id
                    join module in context.Modules
                        on assignment.ModuleId equals (int?)module.Id into moduleGroup
                    from module in moduleGroup.DefaultIfEmpty()
                    where submission.Id == submissionId
                          && course.TrainerId == trainerId
                          && course.IsActive
                          && assignment.IsActive
                          && enrollment.Status == "Active"
                          && student.IsActive
                    select new TrainerAssignmentGradingDetailDto
                    {
                        SubmissionId = submission.Id,
                        AssignmentId = assignment.Id,
                        StudentId = student.Id,

                        StudentName = student.Name,
                        AssignmentTitle = assignment.Title,
                        CourseTitle = course.Title,
                        ModuleTitle = module == null ? "Course Level" : module.Title,

                        SubmittedAt = submission.SubmittedAt,

                        TotalMarks = assignment.Marks,
                        ObtainedMarks = submission.ObtainedMarks,
                        Feedback = submission.Feedback ?? "",

                        IsGraded = submission.IsGraded,

                        SubmittedFilePath = submission.SubmittedFilePath ?? "",
                        SubmittedText = submission.SubmittedText ?? ""
                    }
                ).FirstOrDefaultAsync();

            if (detail == null)
            {
                return null;
            }

            ApplyDetailComputedFields(detail);

            return detail;
        }

        public async Task<TrainerAssignmentGradingSaveResultDto?> SaveGradeAsync(
            int trainerId,
            int submissionId,
            TrainerAssignmentGradingSaveDto dto)
        {
            var submission = await context.AssignmentSubmissions
                .Include(x => x.Assignment)
                .ThenInclude(x => x!.Course)
                .FirstOrDefaultAsync(x =>
                    x.Id == submissionId &&
                    x.Assignment != null &&
                    x.Assignment.Course != null &&
                    x.Assignment.Course.TrainerId == trainerId &&
                    x.Assignment.Course.IsActive &&
                    x.Assignment.IsActive);

            if (submission == null || submission.Assignment == null)
            {
                return null;
            }

            var isActiveEnrollment = await context.CourseEnrollments.AnyAsync(x =>
                x.StudentId == submission.StudentId &&
                x.CourseId == submission.Assignment.CourseId &&
                x.Status == "Active");

            if (!isActiveEnrollment)
            {
                return null;
            }

            if (dto.ObtainedMarks < 0 || dto.ObtainedMarks > submission.Assignment.Marks)
            {
                throw new InvalidOperationException(
                    $"Awarded marks must be between 0 and {submission.Assignment.Marks}.");
            }

            submission.ObtainedMarks = dto.ObtainedMarks;
            submission.Feedback = string.IsNullOrWhiteSpace(dto.Feedback)
                ? null
                : dto.Feedback.Trim();

            submission.IsGraded = true;
            submission.GradedByUserId = trainerId;
            submission.GradedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new TrainerAssignmentGradingSaveResultDto
            {
                Success = true,
                Message = "Grade saved successfully.",
                SubmissionId = submission.Id,
                ObtainedMarks = dto.ObtainedMarks,
                TotalMarks = submission.Assignment.Marks,
                ScoreText = $"{dto.ObtainedMarks}/{submission.Assignment.Marks}"
            };
        }

        public async Task<TrainerAssignmentGradingSaveResultDto?> MarkMissingSubmissionZeroAsync(
            int trainerId,
            TrainerAssignmentGradingMarkZeroDto dto)
        {
            if (dto.AssignmentId <= 0 || dto.StudentId <= 0)
            {
                return null;
            }

            var assignment = await context.Assignments
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x =>
                    x.Id == dto.AssignmentId &&
                    x.Course != null &&
                    x.Course.TrainerId == trainerId &&
                    x.Course.IsActive &&
                    x.IsActive);

            if (assignment == null || assignment.Course == null)
            {
                return null;
            }

            var isActiveEnrollment = await context.CourseEnrollments.AnyAsync(x =>
                x.StudentId == dto.StudentId &&
                x.CourseId == assignment.CourseId &&
                x.Status == "Active");

            if (!isActiveEnrollment)
            {
                return null;
            }

            var studentExists = await context.Users.AnyAsync(x =>
                x.Id == dto.StudentId &&
                x.IsActive);

            if (!studentExists)
            {
                return null;
            }

            if (DateTime.Now <= assignment.DueDateTime)
            {
                throw new InvalidOperationException(
                    "This assignment is not overdue yet. You can mark it as 0 only after the due date has passed.");
            }

            var existingSubmission = await context.AssignmentSubmissions
                .FirstOrDefaultAsync(x =>
                    x.AssignmentId == dto.AssignmentId &&
                    x.StudentId == dto.StudentId);

            if (existingSubmission != null)
            {
                throw new InvalidOperationException(
                    "This student already has a submission record for this assignment.");
            }

            var submission = new AssignmentSubmission
            {
                AssignmentId = dto.AssignmentId,
                StudentId = dto.StudentId,
                SubmittedFilePath = null,
                SubmittedText = null,
                SubmittedAt = assignment.DueDateTime,
                ObtainedMarks = 0,
                Feedback = MissingZeroFeedback,
                IsGraded = true,
                GradedByUserId = trainerId,
                GradedAt = DateTime.UtcNow
            };

            context.AssignmentSubmissions.Add(submission);
            await context.SaveChangesAsync();

            return new TrainerAssignmentGradingSaveResultDto
            {
                Success = true,
                Message = "Missing submission marked as 0.",
                SubmissionId = submission.Id,
                ObtainedMarks = 0,
                TotalMarks = assignment.Marks,
                ScoreText = $"0/{assignment.Marks}"
            };
        }

        private async Task<TrainerAssignmentGradingFilterOptionsDto> GetFilterOptionsAsync(
            int trainerId)
        {
            var courses = await context.Courses
                .Where(x => x.TrainerId == trainerId && x.IsActive)
                .OrderBy(x => x.Title)
                .Select(x => new TrainerAssignmentGradingCourseOptionDto
                {
                    CourseId = x.Id,
                    CourseTitle = x.Title
                })
                .ToListAsync();

            var modules = await context.Modules
                .Where(x =>
                    x.Course != null &&
                    x.Course.TrainerId == trainerId &&
                    x.Course.IsActive &&
                    x.IsActive)
                .OrderBy(x => x.CourseId)
                .ThenBy(x => x.DisplayOrder)
                .ThenBy(x => x.Title)
                .Select(x => new TrainerAssignmentGradingModuleOptionDto
                {
                    ModuleId = x.Id,
                    CourseId = x.CourseId,
                    ModuleTitle = x.Title
                })
                .ToListAsync();

            return new TrainerAssignmentGradingFilterOptionsDto
            {
                Courses = courses,
                Modules = modules
            };
        }

        private void ApplyListComputedFields(
            TrainerAssignmentGradingSubmissionItemDto item)
        {
            item.DueDateText = item.DueDateTime.ToString("dd MMM, hh:mm tt");
            item.IsDuePassed = DateTime.Now > item.DueDateTime;
            item.IsMarkedZeroMissing = IsMarkedZeroMissing(
                item.IsGraded,
                item.ObtainedMarks,
                item.SubmittedFilePath,
                item.SubmittedText,
                item.Feedback);

            item.SubmittedAtText = item.SubmittedAt.HasValue && !item.IsMarkedZeroMissing
                ? item.SubmittedAt.Value.ToString("dd MMM, hh:mm tt")
                : "—";

            if (!item.IsSubmitted)
            {
                item.Status = "Not Submitted";
                item.StatusCssClass = "trainer-pill-danger";
                item.ScoreText = $"--/{item.TotalMarks}";
                item.ScoreCssClass = "trainer-score-muted";

                if (item.IsDuePassed)
                {
                    item.ActionText = "Mark 0";
                    item.ActionUrl = "";
                    item.CanGrade = false;
                    item.CanEdit = false;
                    item.CanMarkZero = true;
                }
                else
                {
                    item.ActionText = "—";
                    item.ActionUrl = "";
                    item.CanGrade = false;
                    item.CanEdit = false;
                    item.CanMarkZero = false;
                }

                return;
            }

            if (item.IsGraded)
            {
                item.Status = "Graded";
                item.StatusCssClass = "pill pill-green";
                item.ScoreText = $"{item.ObtainedMarks ?? 0}/{item.TotalMarks}";
                item.ScoreCssClass = GetScoreCssClass(item.ObtainedMarks ?? 0, item.TotalMarks);
                item.ActionText = "Edit";
                item.ActionUrl = $"/Trainer/Submissions/Edit/{item.SubmissionId}";
                item.CanGrade = false;
                item.CanEdit = true;
                item.CanMarkZero = false;
                return;
            }

            item.Status = "Pending";
            item.StatusCssClass = "pill pill-yellow";
            item.ScoreText = $"--/{item.TotalMarks}";
            item.ScoreCssClass = "trainer-score-muted";
            item.ActionText = "Grade";
            item.ActionUrl = $"/Trainer/Submissions/Grade/{item.SubmissionId}";
            item.CanGrade = true;
            item.CanEdit = false;
            item.CanMarkZero = false;
        }

        private void ApplyDetailComputedFields(
            TrainerAssignmentGradingDetailDto detail)
        {
            detail.IsMarkedZeroMissing = IsMarkedZeroMissing(
                detail.IsGraded,
                detail.ObtainedMarks,
                detail.SubmittedFilePath,
                detail.SubmittedText,
                detail.Feedback);

            detail.SubmittedAtText = detail.IsMarkedZeroMissing
                ? "—"
                : detail.SubmittedAt.ToString("dd MMM, hh:mm tt");

            if (detail.IsGraded)
            {
                detail.Status = "Graded";
                detail.StatusCssClass = "pill pill-green";
                detail.CurrentScoreText = $"{detail.ObtainedMarks ?? 0}/{detail.TotalMarks}";
                detail.CurrentScoreMeta = GetResultLabel(detail.ObtainedMarks ?? 0, detail.TotalMarks);
            }
            else
            {
                detail.Status = "Pending Review";
                detail.StatusCssClass = "pill pill-yellow";
                detail.CurrentScoreText = $"--/{detail.TotalMarks}";
                detail.CurrentScoreMeta = "Pending";
            }

            detail.SubmittedFileName = GetFileName(detail.SubmittedFilePath);
            detail.FileExtension = GetFileExtension(detail.SubmittedFilePath);
            detail.FileViewType = GetFileViewType(detail.FileExtension);
            detail.CanViewFile = !string.IsNullOrWhiteSpace(detail.FileViewType);
            detail.CanDownloadFile = !string.IsNullOrWhiteSpace(detail.SubmittedFilePath);
        }

        private bool IsMarkedZeroMissing(
            bool isGraded,
            int? obtainedMarks,
            string submittedFilePath,
            string submittedText,
            string feedback)
        {
            return isGraded &&
                   obtainedMarks == 0 &&
                   string.IsNullOrWhiteSpace(submittedFilePath) &&
                   string.IsNullOrWhiteSpace(submittedText) &&
                   string.Equals(
                       feedback?.Trim(),
                       MissingZeroFeedback,
                       StringComparison.OrdinalIgnoreCase);
        }

        private string GetScoreCssClass(int obtainedMarks, int totalMarks)
        {
            if (totalMarks <= 0)
            {
                return "trainer-score-muted";
            }

            var percentage = (obtainedMarks * 100.0) / totalMarks;

            if (percentage >= 80)
            {
                return "trainer-score-good";
            }

            if (percentage >= 50)
            {
                return "trainer-score-warn";
            }

            return "trainer-score-muted";
        }

        private string GetResultLabel(int obtainedMarks, int totalMarks)
        {
            if (totalMarks <= 0)
            {
                return "Needs Improvement";
            }

            var percentage = (obtainedMarks * 100.0) / totalMarks;

            if (percentage >= 80)
            {
                return "Excellent Work";
            }

            if (percentage >= 50)
            {
                return "Good Attempt";
            }

            return "Needs Improvement";
        }

        private string GetFileName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return "No file uploaded";
            }

            return Path.GetFileName(filePath);
        }

        private string GetFileExtension(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return "";
            }

            return Path.GetExtension(filePath)
                .Replace(".", "")
                .Trim()
                .ToLower();
        }

        private string GetFileViewType(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return "";
            }

            var imageExtensions = new[] { "jpg", "jpeg", "png", "gif", "webp" };
            var videoExtensions = new[] { "mp4", "webm", "ogg" };

            if (extension == "pdf")
            {
                return "pdf";
            }

            if (imageExtensions.Contains(extension))
            {
                return "image";
            }

            if (videoExtensions.Contains(extension))
            {
                return "video";
            }

            return "";
        }
    }
}