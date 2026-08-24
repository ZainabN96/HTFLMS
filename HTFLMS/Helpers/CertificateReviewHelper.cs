using HTFLMS.Data;
using HTFLMS.Dtos.CertificateReview;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Helper
{
    public class CertificateReviewHelper
    {
        private const string MissingZeroFeedback = "No submission received before the due date.";

        private readonly ApplicationDbContext context;

        public CertificateReviewHelper(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<CertificateReviewListDto?> BuildReviewAsync(
            int courseId,
            string? search,
            string? certificateStatus,
            List<CertificateReviewCourseOptionDto> courseOptions)
        {
            var course = await context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == courseId && x.IsActive);

            if (course == null)
                return null;

            var isCourseEnded = course.BatchEndDate.HasValue &&
                                course.BatchEndDate.Value.Date <= DateTime.UtcNow.Date;

            var assignments = await context.Assignments
                .AsNoTracking()
                .Include(x => x.Module)
                .Where(x => x.CourseId == courseId && x.IsActive)
                .OrderBy(x => x.Module == null ? 0 : x.Module.DisplayOrder)
                .ThenBy(x => x.DueDateTime)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var enrollmentQuery = context.CourseEnrollments
                .AsNoTracking()
                .Include(x => x.Student)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.Status == "Active" &&
                    x.Student != null &&
                    x.Student.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                enrollmentQuery = enrollmentQuery.Where(x =>
                    x.Student!.Name.ToLower().Contains(keyword) ||
                    x.Student.Email.ToLower().Contains(keyword));
            }

            var enrollments = await enrollmentQuery
                .OrderBy(x => x.Student!.Name)
                .ToListAsync();

            var studentIds = enrollments
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var assignmentIds = assignments
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            var generatedCertificateStudentIds = await context.Certificates
                .AsNoTracking()
                .Where(x =>
                    x.CourseId == courseId &&
                    studentIds.Contains(x.StudentId))
                .Select(x => x.StudentId)
                .Distinct()
                .ToListAsync();

            var generatedCertificateStudentSet = new HashSet<int>(generatedCertificateStudentIds);

            var submissions = await context.AssignmentSubmissions
                .AsNoTracking()
                .Where(x =>
                    studentIds.Contains(x.StudentId) &&
                    assignmentIds.Contains(x.AssignmentId))
                .ToListAsync();

            var submissionLookup = submissions
                .GroupBy(x => new { x.StudentId, x.AssignmentId })
                .ToDictionary(
                    g => $"{g.Key.StudentId}_{g.Key.AssignmentId}",
                    g => g.OrderByDescending(x => x.SubmittedAt).First());

            var certificateRequests = await context.CertificateRequests
                .AsNoTracking()
                .Where(x =>
                    x.CourseId == courseId &&
                    studentIds.Contains(x.StudentId))
                .OrderByDescending(x => x.RequestedAt)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            var latestRequestLookup = certificateRequests
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.First());

            var assignmentColumns = assignments
                .Select((assignment, index) => new CertificateReviewAssignmentColumnDto
                {
                    AssignmentId = assignment.Id,
                    AssignmentTitle = assignment.Title,
                    ShortTitle = GetShortAssignmentTitle(assignment.Title, index),
                    TotalMarks = assignment.Marks,
                    DueDateTime = assignment.DueDateTime,
                    DueDateText = assignment.DueDateTime.ToString("dd MMM, yyyy")
                })
                .ToList();

            var rows = new List<CertificateReviewStudentRowDto>();

            foreach (var enrollment in enrollments)
            {
                if (enrollment.Student == null)
                    continue;

                latestRequestLookup.TryGetValue(enrollment.StudentId, out var latestRequest);

                var row = BuildStudentRow(
                    course,
                    enrollment,
                    assignments,
                    submissionLookup,
                    latestRequest,
                    isCourseEnded,
                    generatedCertificateStudentSet.Contains(enrollment.StudentId));

                rows.Add(row);
            }

            rows = ApplyCertificateStatusFilter(rows, certificateStatus);

            return new CertificateReviewListDto
            {
                Summary = BuildSummary(rows),
                Filters = new CertificateReviewFilterDto
                {
                    Courses = courseOptions
                },
                SelectedCourseId = course.Id,
                SelectedCourseTitle = course.Title,
                IsCourseEnded = isCourseEnded,
                Assignments = assignmentColumns,
                Students = rows,
                EmptyMessage = "No students found for this course."
            };
        }

        public async Task<int?> GetRequestCourseIdAsync(int requestId)
        {
            var request = await context.CertificateRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == requestId);

            return request?.CourseId;
        }

        public async Task<CertificateReviewActionResultDto> ApproveRequestAsync(
            int reviewerId,
            int requestId)
        {
            var request = await context.CertificateRequests
                .Include(x => x.Course)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == requestId);

            if (request == null || request.Course == null)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Certificate request was not found."
                };
            }

            if (!request.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Only pending certificate requests can be approved."
                };
            }

            request.Status = "Approved";
            request.ApprovedByUserId = reviewerId;
            request.ApprovedAt = DateTime.UtcNow;

            context.Notifications.Add(new Notification
            {
                UserId = request.StudentId,
                Title = "Certificate Approved",
                Message = $"Your certificate for {request.Course.Title} has been approved.",
                Type = "Certificate",
                RedirectUrl = "/Student/Certificates"
            });

            await context.SaveChangesAsync();

            return new CertificateReviewActionResultDto
            {
                Success = true,
                Message = "Certificate request approved successfully."
            };
        }

        public async Task<CertificateReviewActionResultDto> RejectRequestAsync(
            int reviewerId,
            int requestId)
        {
            var request = await context.CertificateRequests
                .Include(x => x.Course)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Id == requestId);

            if (request == null || request.Course == null)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Certificate request was not found."
                };
            }

            if (!request.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Only pending certificate requests can be rejected."
                };
            }

            request.Status = "Rejected";

            context.Notifications.Add(new Notification
            {
                UserId = request.StudentId,
                Title = "Certificate Request Rejected",
                Message = $"Your certificate request for {request.Course.Title} was rejected. Please complete your pending assignments/submissions and apply again.",
                Type = "Certificate",
                RedirectUrl = "/Student/Certificates"
            });

            await context.SaveChangesAsync();

            return new CertificateReviewActionResultDto
            {
                Success = true,
                Message = "Certificate request rejected successfully."
            };
        }

        public async Task<CertificateReviewActionResultDto> UpdateDeliveryModeAsync(
            int updatedByUserId,
            int enrollmentId,
            string deliveryMode)
        {
            var normalizedDeliveryMode = NormalizeDeliveryModeForUpdate(deliveryMode);

            if (normalizedDeliveryMode == null)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Delivery mode must be Online or Onsite."
                };
            }

            var enrollment = await context.CourseEnrollments
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.Id == enrollmentId);

            if (enrollment == null || enrollment.Course == null)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Enrollment record was not found."
                };
            }

            var certificateAlreadyGenerated = await context.Certificates
                .AnyAsync(x =>
                    x.StudentId == enrollment.StudentId &&
                    x.CourseId == enrollment.CourseId);

            if (certificateAlreadyGenerated)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Delivery mode cannot be changed because certificate has already been generated."
                };
            }

            enrollment.DeliveryMode = normalizedDeliveryMode;
            enrollment.DeliveryModeUpdatedByUserId = updatedByUserId;
            enrollment.DeliveryModeUpdatedAt = DateTime.UtcNow;

            context.CourseEnrollments.Update(enrollment);
            await context.SaveChangesAsync();

            return new CertificateReviewActionResultDto
            {
                Success = true,
                Message = "Delivery mode updated successfully."
            };
        }

        private CertificateReviewStudentRowDto BuildStudentRow(
            Course course,
            CourseEnrollment enrollment,
            List<Assignment> assignments,
            Dictionary<string, AssignmentSubmission> submissionLookup,
            CertificateRequest? latestRequest,
            bool isCourseEnded,
            bool certificateAlreadyGenerated)
        {
            var student = enrollment.Student!;

            var cells = new List<CertificateReviewAssignmentCellDto>();

            foreach (var assignment in assignments)
            {
                var key = $"{student.Id}_{assignment.Id}";
                submissionLookup.TryGetValue(key, out var submission);

                cells.Add(BuildAssignmentCell(assignment, submission));
            }

            var includedCells = cells
                .Where(x => x.IsGraded)
                .ToList();

            var totalMarks = includedCells.Sum(x => x.TotalMarks);
            var obtainedMarks = includedCells.Sum(x => x.ObtainedMarks ?? 0);

            var percentage = totalMarks > 0
                ? Math.Round((obtainedMarks * 100m) / totalMarks, 1)
                : 0m;

            var deliveryMode = NormalizeDeliveryMode(enrollment.DeliveryMode);

            var row = new CertificateReviewStudentRowDto
            {
                StudentId = student.Id,
                StudentName = student.Name,
                CourseId = course.Id,
                CourseTitle = course.Title,

                EnrollmentId = enrollment.Id,
                DeliveryMode = deliveryMode,
                DeliveryModeText = deliveryMode,
                CanUpdateDeliveryMode = !certificateAlreadyGenerated,

                AssignmentCells = cells,
                TotalMarks = totalMarks,
                ObtainedMarks = obtainedMarks,
                OverallPercentage = percentage,
                OverallText = totalMarks > 0 ? $"{percentage:0.#}%" : "—",
                OverallCssClass = totalMarks > 0 ? GetScoreCssClass(percentage) : "trainer-score-muted"
            };

            ApplyStanding(row);
            ApplyCertificateStatus(row, latestRequest, isCourseEnded);

            return row;
        }

        private CertificateReviewAssignmentCellDto BuildAssignmentCell(
            Assignment assignment,
            AssignmentSubmission? submission)
        {
            if (submission == null)
            {
                var isDuePassed = DateTime.Now > assignment.DueDateTime;

                if (isDuePassed)
                {
                    return new CertificateReviewAssignmentCellDto
                    {
                        AssignmentId = assignment.Id,
                        AssignmentTitle = assignment.Title,
                        TotalMarks = assignment.Marks,
                        ObtainedMarks = null,
                        ValueText = "Missing",
                        ValueCssClass = "pill trainer-grade-pill-danger",
                        Status = "Missing",
                        StatusCssClass = "pill trainer-grade-pill-danger",
                        IsScore = false,
                        IsSubmitted = false,
                        IsGraded = false,
                        IsMissing = true,
                        IsPending = false
                    };
                }

                return new CertificateReviewAssignmentCellDto
                {
                    AssignmentId = assignment.Id,
                    AssignmentTitle = assignment.Title,
                    TotalMarks = assignment.Marks,
                    ObtainedMarks = null,
                    ValueText = "Pending",
                    ValueCssClass = "pill trainer-grade-pill-warn",
                    Status = "Pending Submission",
                    StatusCssClass = "pill trainer-grade-pill-warn",
                    IsScore = false,
                    IsSubmitted = false,
                    IsGraded = false,
                    IsMissing = false,
                    IsPending = true
                };
            }

            if (!submission.IsGraded)
            {
                return new CertificateReviewAssignmentCellDto
                {
                    AssignmentId = assignment.Id,
                    AssignmentTitle = assignment.Title,
                    TotalMarks = assignment.Marks,
                    ObtainedMarks = null,
                    ValueText = "Not Graded",
                    ValueCssClass = "pill trainer-grade-pill-warn",
                    Status = "Not Graded",
                    StatusCssClass = "pill trainer-grade-pill-warn",
                    IsScore = false,
                    IsSubmitted = true,
                    IsGraded = false,
                    IsMissing = false,
                    IsPending = true
                };
            }

            var obtainedMarks = submission.ObtainedMarks ?? 0;
            var percentage = assignment.Marks > 0
                ? Math.Round((obtainedMarks * 100m) / assignment.Marks, 1)
                : 0m;

            var isMarkedZeroMissing = IsMarkedZeroMissing(submission);

            return new CertificateReviewAssignmentCellDto
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                TotalMarks = assignment.Marks,
                ObtainedMarks = obtainedMarks,
                ValueText = isMarkedZeroMissing ? "0(M)" : obtainedMarks.ToString(),
                ValueCssClass = isMarkedZeroMissing
                    ? "trainer-grade-danger"
                    : GetScoreCssClass(percentage),
                Status = isMarkedZeroMissing ? "Marked 0 Missing" : "Graded",
                StatusCssClass = isMarkedZeroMissing
                    ? "pill trainer-grade-pill-danger"
                    : "pill trainer-grade-pill-good",
                IsScore = true,
                IsSubmitted = true,
                IsGraded = true,
                IsMissing = isMarkedZeroMissing,
                IsPending = false
            };
        }

        private void ApplyStanding(CertificateReviewStudentRowDto row)
        {
            if (!row.AssignmentCells.Any())
            {
                row.StandingText = "No Assignments";
                row.StandingCssClass = "pill trainer-grade-pill-warn";
                return;
            }

            var hasMissing = row.AssignmentCells.Any(x => x.IsMissing);
            var hasPending = row.AssignmentCells.Any(x => x.IsPending);
            var hasIncludedGrades = row.AssignmentCells.Any(x => x.IsGraded);

            if (hasMissing)
            {
                row.StandingText = "At risk";
                row.StandingCssClass = "pill trainer-grade-pill-risk";
                return;
            }

            if (!hasIncludedGrades)
            {
                row.StandingText = "Pending";
                row.StandingCssClass = "pill trainer-grade-pill-warn";
                return;
            }

            if (row.OverallPercentage < 50)
            {
                row.StandingText = "At risk";
                row.StandingCssClass = "pill trainer-grade-pill-risk";
                return;
            }

            if (hasPending)
            {
                row.StandingText = "Pending";
                row.StandingCssClass = "pill trainer-grade-pill-warn";
                return;
            }

            if (row.OverallPercentage >= 80)
            {
                row.StandingText = "Completed";
                row.StandingCssClass = "pill trainer-grade-pill-good";
                return;
            }

            row.StandingText = "Good";
            row.StandingCssClass = "pill trainer-grade-pill-good";
        }

        private void ApplyCertificateStatus(
            CertificateReviewStudentRowDto row,
            CertificateRequest? latestRequest,
            bool isCourseEnded)
        {
            if (!isCourseEnded)
            {
                row.CertificateStatus = "InProgress";
                row.CertificateStatusText = "In Progress";
                row.CertificateStatusCssClass = "pill trainer-grade-pill-warn";
                row.CanApprove = false;
                row.CanReject = false;
                return;
            }

            if (latestRequest == null)
            {
                row.CertificateStatus = "NotApplied";
                row.CertificateStatusText = "Not Applied";
                row.CertificateStatusCssClass = "pill trainer-grade-pill-warn";
                row.CanApprove = false;
                row.CanReject = false;
                return;
            }

            row.CertificateRequestId = latestRequest.Id;

            if (latestRequest.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                row.CertificateStatus = "Pending";
                row.CertificateStatusText = "Pending Approval";
                row.CertificateStatusCssClass = "pill trainer-grade-pill-warn";
                row.CanApprove = true;
                row.CanReject = true;
                return;
            }

            if (latestRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                row.CertificateStatus = "Approved";
                row.CertificateStatusText = "Approved";
                row.CertificateStatusCssClass = "pill trainer-grade-pill-good";
                row.CanApprove = false;
                row.CanReject = false;
                return;
            }

            if (latestRequest.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                row.CertificateStatus = "Rejected";
                row.CertificateStatusText = "Rejected";
                row.CertificateStatusCssClass = "pill trainer-grade-pill-danger";
                row.CanApprove = false;
                row.CanReject = false;
                return;
            }

            row.CertificateStatus = latestRequest.Status;
            row.CertificateStatusText = latestRequest.Status;
            row.CertificateStatusCssClass = "pill trainer-grade-pill-warn";
            row.CanApprove = false;
            row.CanReject = false;
        }

        private CertificateReviewSummaryDto BuildSummary(List<CertificateReviewStudentRowDto> rows)
        {
            if (!rows.Any())
            {
                return new CertificateReviewSummaryDto();
            }

            return new CertificateReviewSummaryDto
            {
                TotalStudents = rows.Count,

                OverallClassAverage = Math.Round(rows.Average(x => x.OverallPercentage), 1),
                HighestScore = (int)Math.Round(rows.Max(x => x.OverallPercentage), 0),
                LowestScore = (int)Math.Round(rows.Min(x => x.OverallPercentage), 0),
                AtRiskStudents = rows.Count(x => x.StandingText == "At risk"),

                InProgress = rows.Count(x => x.CertificateStatus == "InProgress"),
                NotApplied = rows.Count(x => x.CertificateStatus == "NotApplied"),
                PendingRequests = rows.Count(x => x.CertificateStatus == "Pending"),
                Approved = rows.Count(x => x.CertificateStatus == "Approved"),
                Rejected = rows.Count(x => x.CertificateStatus == "Rejected")
            };
        }

        private List<CertificateReviewStudentRowDto> ApplyCertificateStatusFilter(
            List<CertificateReviewStudentRowDto> rows,
            string? certificateStatus)
        {
            if (string.IsNullOrWhiteSpace(certificateStatus) ||
                certificateStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                return rows;
            }

            var normalizedStatus = NormalizeStatus(certificateStatus);

            return rows
                .Where(x => NormalizeStatus(x.CertificateStatus) == normalizedStatus ||
                            NormalizeStatus(x.CertificateStatusText) == normalizedStatus)
                .ToList();
        }

        private bool IsMarkedZeroMissing(AssignmentSubmission submission)
        {
            return submission.IsGraded &&
                   submission.ObtainedMarks == 0 &&
                   string.IsNullOrWhiteSpace(submission.SubmittedFilePath) &&
                   string.IsNullOrWhiteSpace(submission.SubmittedText) &&
                   string.Equals(
                       submission.Feedback?.Trim(),
                       MissingZeroFeedback,
                       StringComparison.OrdinalIgnoreCase);
        }

        private string GetScoreCssClass(decimal percentage)
        {
            if (percentage >= 80)
                return "trainer-grade-good";

            if (percentage >= 50)
                return "trainer-grade-warn";

            return "trainer-grade-danger";
        }

        private string GetShortAssignmentTitle(string title, int index)
        {
            return $"A{index + 1}";
        }

        private string NormalizeStatus(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Trim().Replace(" ", "").ToLower();
        }

        private static string NormalizeDeliveryMode(string? deliveryMode)
        {
            return string.Equals(deliveryMode?.Trim(), "Online", StringComparison.OrdinalIgnoreCase)
                ? "Online"
                : "Onsite";
        }

        private static string? NormalizeDeliveryModeForUpdate(string? deliveryMode)
        {
            if (string.Equals(deliveryMode?.Trim(), "Online", StringComparison.OrdinalIgnoreCase))
                return "Online";

            if (string.Equals(deliveryMode?.Trim(), "Onsite", StringComparison.OrdinalIgnoreCase))
                return "Onsite";

            return null;
        }
    }
}