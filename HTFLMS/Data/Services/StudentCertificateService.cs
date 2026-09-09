using HTFLMS.Data.IServices;
using HTFLMS.Dtos.StudentCertificate;
using HTFLMS.Helper;
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

            var courseIds = enrollments
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            var requests = await context.CertificateRequests
                .Where(r =>
                    r.StudentId == studentId &&
                    courseIds.Contains(r.CourseId))
                .OrderByDescending(r => r.RequestedAt)
                .ThenByDescending(r => r.Id)
                .ToListAsync();

            var latestRequests = requests
                .GroupBy(r => r.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var generatedCertificates = await context.Certificates
                .AsNoTracking()
                .Where(c =>
                    c.StudentId == studentId &&
                    courseIds.Contains(c.CourseId))
                .OrderByDescending(c => c.GeneratedAt)
                .ThenByDescending(c => c.Id)
                .ToListAsync();

            var generatedCertificateLookup = generatedCertificates
                .GroupBy(c => c.CourseId)
                .ToDictionary(g => g.Key, g => g.First());

            var result = new List<StudentCertificateDto>();
            var today = DateTime.UtcNow.Date;

            foreach (var enrollment in enrollments)
            {
                var course = enrollment.Course;
                if (course == null) continue;

                latestRequests.TryGetValue(course.Id, out var latestRequest);
                generatedCertificateLookup.TryGetValue(course.Id, out var generatedCertificate);

                var isCourseEnded = course.BatchEndDate.HasValue &&
                                    course.BatchEndDate.Value.Date <= today;

                var dto = BuildCertificateDto(
                    enrollment,
                    latestRequest,
                    generatedCertificate,
                    isCourseEnded);

                result.Add(dto);
            }

            return result;
        }

        public async Task<StudentCertificateApplyResultDto> ApplyAsync(
            int studentId,
            int courseId)
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

            var generatedCertificateExists = await context.Certificates
                .AsNoTracking()
                .AnyAsync(c =>
                    c.StudentId == studentId &&
                    c.CourseId == courseId);

            if (generatedCertificateExists)
            {
                return new StudentCertificateApplyResultDto
                {
                    Success = false,
                    Message = "Your certificate has already been generated."
                };
            }

            var latestRequest = await context.CertificateRequests
                .Where(r =>
                    r.StudentId == studentId &&
                    r.CourseId == courseId)
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
                    Message = "Your certificate has already been approved and is being prepared."
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
            var certificate = await context.Certificates
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.CertificateRequestId == certificateRequestId &&
                    c.StudentId == studentId &&
                    !string.IsNullOrWhiteSpace(c.CertificateFilePath));

            if (certificate == null)
                return null;

            return new StudentCertificateDetailDto
            {
                CertificateRequestId = certificate.CertificateRequestId,
                CertificateRecordId = certificate.Id,

                CertificateId = certificate.CertificateId,
                StudentName = CertificateGenerationHelper.BuildStudentDisplayName(
                    certificate.TitlePrefixSnapshot,
                    certificate.StudentNameSnapshot),

                CourseTitle = certificate.CourseTitleSnapshot,
                BatchNumber = certificate.BatchNumberSnapshot,
                DurationText = certificate.DurationSnapshot,
                BatchStartDateText = FormatDate(certificate.BatchStartDateSnapshot),
                BatchEndDateText = FormatDate(certificate.BatchEndDateSnapshot),

                IssueDateText = FormatDate(certificate.IssueDate),
                Status = "Generated",
                DeliveryMode = certificate.DeliveryMode,
                DownloadUrl = certificate.CertificateFilePath,
                CertificateFilePath = certificate.CertificateFilePath
            };
        }

        private StudentCertificateDto BuildCertificateDto(
            CourseEnrollment enrollment,
            CertificateRequest? latestRequest,
            Certificate? generatedCertificate,
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

            if (generatedCertificate != null &&
                !string.IsNullOrWhiteSpace(generatedCertificate.CertificateFilePath))
            {
                dto.CertificateRequestId = generatedCertificate.CertificateRequestId;
                dto.CertificateRecordId = generatedCertificate.Id;

                dto.Status = "Generated";
                dto.StatusText = "Certificate Ready";
                dto.StatusCssClass = "student-course-tag";

                dto.IsCertificateGenerated = true;
                dto.CertificateNumber = generatedCertificate.CertificateId;
                dto.CertificateFilePath = generatedCertificate.CertificateFilePath;

                dto.CanApply = false;
                dto.CanView = true;
                dto.CanDownload = true;

                dto.ButtonText = "View Certificate";
                dto.IssueDateText = FormatDate(generatedCertificate.IssueDate);
                dto.ApprovedAtText = FormatDate(generatedCertificate.IssueDate);

                dto.ViewUrl = $"/Student/Certificates/ViewCertificate/{generatedCertificate.CertificateRequestId}";
                dto.DownloadUrl = generatedCertificate.CertificateFilePath;

                dto.Message = $"Your certificate is ready. Certificate ID: {generatedCertificate.CertificateId}";
                return dto;
            }

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
                dto.Message = "Your certificate request is under trainer/admin review.";
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
                dto.Status = "Processing";
                dto.StatusText = "Certificate Processing";
                dto.StatusCssClass = "student-course-tag";
                dto.CanApply = false;
                dto.CanView = false;
                dto.CanDownload = false;
                dto.ButtonText = "Certificate Processing";
                dto.ApprovedAtText = FormatDate(latestRequest.ApprovedAt ?? latestRequest.RequestedAt);
                dto.Message = "Your certificate has been approved and is being prepared.";
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

            var name = !string.IsNullOrWhiteSpace(user.Name)
                ? user.Name
                : user.Email;

            return CertificateGenerationHelper.BuildStudentDisplayName(
                user.TitlePrefix,
                name);
        }
    }
}