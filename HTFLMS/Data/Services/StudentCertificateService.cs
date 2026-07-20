using HTFLMS.Data.IServices;
using HTFLMS.Dtos.StudentCertificate;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class StudentCertificateService : IStudentCertificateService
    {
        private readonly ApplicationDbContext context;

        public StudentCertificateService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<StudentCertificateDto>> GetCertificatesAsync(int studentId)
        {
            var enrollments = await context.CourseEnrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .Where(e =>
                    e.StudentId == studentId &&
                    e.Status != "Dropped" &&
                    e.Course != null &&
                    e.Course.CertificateIncluded)
                .OrderByDescending(e => e.Course!.BatchEndDate ?? e.Course.BatchStartDate)
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).Distinct().ToList();

            var requests = await context.CertificateRequests
                .Where(r => r.StudentId == studentId && courseIds.Contains(r.CourseId))
                .OrderByDescending(r => r.RequestedAt)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            var latestRequests = requests
                .GroupBy(r => r.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var result = new List<StudentCertificateDto>();
            var today = DateTime.UtcNow.Date;

            foreach (var enrollment in enrollments)
            {
                var course = enrollment.Course;
                if (course == null) continue;

                latestRequests.TryGetValue(course.Id, out var latestRequest);

                var isCourseEnded = course.BatchEndDate.HasValue &&
                                    course.BatchEndDate.Value.Date <= today;

                var dto = BuildCertificateDto(enrollment, latestRequest, isCourseEnded);
                result.Add(dto);
            }

            return result;
        }

        public async Task<StudentCertificateApplyResultDto> ApplyAsync(int studentId, int courseId)
        {
            var enrollment = await context.CourseEnrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e =>
                    e.StudentId == studentId &&
                    e.CourseId == courseId &&
                    e.Status != "Dropped");

            if (enrollment == null || enrollment.Course == null)
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "Course enrollment was not found."
                };
            }

            var course = enrollment.Course;

            if (!course.CertificateIncluded)
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "Certificate is not available for this course."
                };
            }

            if (!course.BatchEndDate.HasValue)
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "Course end date is not available yet."
                };
            }

            if (course.BatchEndDate.Value.Date > DateTime.UtcNow.Date)
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "You can apply for the certificate after the course end date."
                };
            }

            var latestRequest = await context.CertificateRequests
                .Where(r => r.StudentId == studentId && r.CourseId == courseId)
                .OrderByDescending(r => r.RequestedAt)
                .ThenByDescending(r => r.Id)
                .FirstOrDefaultAsync();

            if (latestRequest != null &&
                latestRequest.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "Your certificate request is already pending approval."
                };
            }

            if (latestRequest != null &&
                latestRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "Your certificate has already been approved."
                };
            }

            var request = new CertificateRequest
            {
                StudentId = studentId,
                CourseId = courseId,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            context.CertificateRequests.Add(request);

            context.Notifications.Add(new Notification
            {
                UserId = course.TrainerId,
                Title = "New Certificate Request",
                Message = $"A student has applied for the certificate of {course.Title}.",
                Type = "Certificate",
                RedirectUrl = "/Trainer/Certificates"
            });

            await context.SaveChangesAsync();

            return new StudentCertificateApplyResultDto
            {
                Success = true,
                Message = "Certificate request submitted successfully."
            };
        }

        public async Task<StudentCertificateDetailDto?> GetCertificateDetailAsync(
            int studentId,
            int certificateRequestId)
        {
            var request = await context.CertificateRequests
                .Include(r => r.Course)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r =>
                    r.Id == certificateRequestId &&
                    r.StudentId == studentId &&
                    r.Status == "Approved");

            if (request == null || request.Course == null)
                return null;

            var issueDate = request.ApprovedAt ?? request.RequestedAt;

            return new StudentCertificateDetailDto
            {
                CertificateRequestId = request.Id,
                CertificateId = $"HCC-{issueDate.Year}-{request.Id:D4}",
                StudentName = GetStudentName(request.Student),
                CourseTitle = request.Course.Title,
                BatchNumber = request.Course.BatchNumber,
                DurationText = request.Course.DurationText,
                BatchStartDateText = FormatDate(request.Course.BatchStartDate),
                BatchEndDateText = FormatDate(request.Course.BatchEndDate),
                IssueDateText = FormatDate(issueDate),
                Status = request.Status,
                DownloadUrl = request.CertificateFilePath
            };
        }

        private StudentCertificateDto BuildCertificateDto(
            CourseEnrollment enrollment,
            CertificateRequest? latestRequest,
            bool isCourseEnded)
        {
            var course = enrollment.Course!;

            var dto = new StudentCertificateDto
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                StudentName = GetStudentName(enrollment.Student),
                CourseImagePath = course.CourseImagePath,
                BatchNumber = course.BatchNumber,
                DurationText = course.DurationText,
                BatchStartDateText = FormatDate(course.BatchStartDate),
                BatchEndDateText = FormatDate(course.BatchEndDate)
            };

            if (!isCourseEnded && latestRequest == null)
            {
                dto.Status = "CourseInProgress";
                dto.StatusText = "Course In Progress";
                dto.StatusCssClass = "student-course-tag";
                dto.CanApply = false;
                dto.ButtonText = "Apply After Course End";
                dto.Message = "You can apply for the certificate after the course end date.";
                return dto;
            }

            if (latestRequest == null)
            {
                dto.Status = "ReadyToApply";
                dto.StatusText = "Ready to Apply";
                dto.StatusCssClass = "student-course-tag";
                dto.CanApply = true;
                dto.ButtonText = "Apply Certificate";
                dto.Message = "Course ended. You can now apply for your certificate.";
                return dto;
            }

            dto.CertificateRequestId = latestRequest.Id;
            dto.RequestedAtText = FormatDate(latestRequest.RequestedAt);

            if (latestRequest.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                dto.Status = "Pending";
                dto.StatusText = "Pending Approval";
                dto.StatusCssClass = "student-course-tag";
                dto.CanApply = false;
                dto.ButtonText = "Pending Approval";
                dto.Message = "Your certificate request is under trainer review.";
                return dto;
            }

            if (latestRequest.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                dto.Status = "Rejected";
                dto.StatusText = "Rejected";
                dto.StatusCssClass = "student-course-tag";
                dto.CanApply = true;
                dto.ButtonText = "Reapply Certificate";
                dto.Message = "Your certificate request was rejected. Please complete your pending assignments/submissions and apply again.";
                return dto;
            }

            if (latestRequest.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                dto.Status = "Approved";
                dto.StatusText = "Certificate Ready";
                dto.StatusCssClass = "student-course-tag";
                dto.CanApply = false;
                dto.CanView = true;
                dto.CanDownload = !string.IsNullOrWhiteSpace(latestRequest.CertificateFilePath);
                dto.ButtonText = "Certificate Ready";
                dto.ApprovedAtText = FormatDate(latestRequest.ApprovedAt ?? latestRequest.RequestedAt);
                dto.ViewUrl = $"/Student/Certificates/ViewCertificate/{latestRequest.Id}";
                dto.DownloadUrl = latestRequest.CertificateFilePath;
                dto.Message = "Your certificate has been approved.";
                return dto;
            }

            dto.Status = latestRequest.Status;
            dto.StatusText = latestRequest.Status;
            dto.StatusCssClass = "student-course-tag";
            dto.CanApply = false;
            dto.ButtonText = latestRequest.Status;
            return dto;
        }

        private static string FormatDate(DateTime date)
        {
            return date.ToString("MMM dd, yyyy");
        }

        private static string? FormatDate(DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("MMM dd, yyyy") : null;
        }

        private static string GetStudentName(User? user)
        {
            if (user == null)
                return "Student";

            return !string.IsNullOrWhiteSpace(user.Email)
                ? user.Email
                : "Student";
        }
    }
}