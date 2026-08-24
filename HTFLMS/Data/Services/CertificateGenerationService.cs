using HTFLMS.Data.IServices;
using HTFLMS.Dtos.CertificateGeneration;
using HTFLMS.Helper;
using HTFLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class CertificateGenerationService : ICertificateGenerationService
    {
        private readonly ApplicationDbContext context;

        public CertificateGenerationService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<CertificateGenerationResultDto> GenerateForCourseAsync(
            int generatedByUserId,
            int courseId)
        {
            var course = await context.Courses
                .FirstOrDefaultAsync(x =>
                    x.Id == courseId &&
                    x.IsActive &&
                    x.CertificateIncluded);

            if (course == null)
            {
                return Fail("Course was not found or certificate is not enabled for this course.");
            }

            if (!course.BatchEndDate.HasValue ||
                course.BatchEndDate.Value.Date > DateTime.UtcNow.Date)
            {
                return Fail("Certificates can only be generated after the course end date.");
            }

            var approvedRequests = await context.CertificateRequests
                .Include(x => x.Student)
                .Include(x => x.Course)
                .Where(x =>
                    x.CourseId == courseId &&
                    x.Status == "Approved" &&
                    x.Student != null &&
                    x.Course != null)
                .OrderBy(x => x.Student!.Name)
                .ThenByDescending(x => x.RequestedAt)
                .ThenByDescending(x => x.Id)
                .ToListAsync();

            if (!approvedRequests.Any())
            {
                return Fail("No approved certificate requests found for this course.");
            }

            approvedRequests = approvedRequests
                .GroupBy(x => x.StudentId)
                .Select(g => g
                    .OrderByDescending(x => x.RequestedAt)
                    .ThenByDescending(x => x.Id)
                    .First())
                .ToList();

            var requestIds = approvedRequests
                .Select(x => x.Id)
                .ToList();

            var studentIds = approvedRequests
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var alreadyGeneratedRequestIds = await context.Certificates
                .AsNoTracking()
                .Where(x => requestIds.Contains(x.CertificateRequestId))
                .Select(x => x.CertificateRequestId)
                .ToListAsync();

            var alreadyGeneratedStudentIdsForCourse = await context.Certificates
                .AsNoTracking()
                .Where(x =>
                    x.CourseId == courseId &&
                    studentIds.Contains(x.StudentId))
                .Select(x => x.StudentId)
                .ToListAsync();

            var alreadyGeneratedRequestSet = alreadyGeneratedRequestIds.ToHashSet();
            var alreadyGeneratedStudentSet = alreadyGeneratedStudentIdsForCourse.ToHashSet();

            var enrollments = await context.CourseEnrollments
                .AsNoTracking()
                .Where(x =>
                    x.CourseId == courseId &&
                    studentIds.Contains(x.StudentId) &&
                    x.Status == "Active")
                .ToListAsync();

            var enrollmentLookup = enrollments
                .GroupBy(x => x.StudentId)
                .ToDictionary(g => g.Key, g => g.First());

            var candidates = new List<CertificateGenerationCandidate>();
            var skippedCount = 0;
            var validationErrors = new List<string>();

            foreach (var request in approvedRequests)
            {
                if (request.Student == null || request.Course == null)
                    continue;

                if (alreadyGeneratedRequestSet.Contains(request.Id) ||
                    alreadyGeneratedStudentSet.Contains(request.StudentId))
                {
                    skippedCount++;
                    continue;
                }

                if (!enrollmentLookup.TryGetValue(request.StudentId, out var enrollment))
                {
                    validationErrors.Add($"{request.Student.Name} does not have an active enrollment for this course.");
                    continue;
                }

                if (!CertificateGenerationHelper.HasValidTitlePrefix(request.Student.TitlePrefix))
                {
                    validationErrors.Add($"Please update title prefix for {request.Student.Name} before generating certificate.");
                    continue;
                }

                var deliveryMode = CertificateGenerationHelper.NormalizeDeliveryMode(enrollment.DeliveryMode);

                candidates.Add(new CertificateGenerationCandidate
                {
                    Request = request,
                    Student = request.Student,
                    Course = request.Course,
                    Enrollment = enrollment,
                    DeliveryMode = deliveryMode
                });
            }

            if (validationErrors.Any())
            {
                return new CertificateGenerationResultDto
                {
                    Success = false,
                    Message = "Certificate generation stopped. Please fix the listed issues first.",
                    SkippedCount = skippedCount,
                    Errors = validationErrors
                };
            }

            if (!candidates.Any())
            {
                return new CertificateGenerationResultDto
                {
                    Success = false,
                    Message = "No approved requests are pending certificate generation.",
                    SkippedCount = skippedCount
                };
            }

            var generatedItems = new List<CertificateGenerationItemDto>();
            var nextBaseNumberTracker = new Dictionary<string, int>();

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var orderedCandidates = candidates
                    .OrderBy(x => x.DeliveryMode)
                    .ThenBy(x => x.Student.Name)
                    .ThenBy(x => x.Request.Id)
                    .ToList();

                foreach (var candidate in orderedCandidates)
                {
                    var studentCertificateNumber = await GetOrCreateStudentCertificateNumberAsync(
                        candidate.Student.Id,
                        candidate.DeliveryMode,
                        generatedByUserId,
                        nextBaseNumberTracker);

                    var existingCertificateCount = await context.Certificates
                        .CountAsync(x =>
                            x.StudentId == candidate.Student.Id &&
                            x.DeliveryMode == candidate.DeliveryMode);

                    var suffix = CertificateGenerationHelper
                        .BuildSuffixFromExistingCertificateCount(existingCertificateCount);

                    var issueDate = DateTime.UtcNow;
                    var certificateYear = issueDate.Year;

                    var certificateNumber = CertificateGenerationHelper.BuildCertificateNumber(
                        certificateYear,
                        studentCertificateNumber.BaseNumber,
                        suffix);

                    var batchNumberText = Convert.ToString(candidate.Course.BatchNumber);

                    var certificate = new Certificate
                    {
                        CertificateRequestId = candidate.Request.Id,
                        StudentId = candidate.Student.Id,
                        CourseId = candidate.Course.Id,
                        StudentCertificateNumber = studentCertificateNumber,

                        DeliveryMode = candidate.DeliveryMode,
                        CertificateYear = certificateYear,
                        BaseNumber = studentCertificateNumber.BaseNumber,
                        Suffix = suffix,
                        CertificateId = certificateNumber,

                        IssueDate = issueDate,
                        CertificateFilePath = "",

                        StudentNameSnapshot = candidate.Student.Name.Trim(),
                        TitlePrefixSnapshot = candidate.Student.TitlePrefix?.Trim(),

                        CourseTitleSnapshot = candidate.Course.Title.Trim(),
                        BatchNumberSnapshot = string.IsNullOrWhiteSpace(batchNumberText) ? null : batchNumberText,
                        DurationSnapshot = candidate.Course.DurationText,
                        BatchStartDateSnapshot = candidate.Course.BatchStartDate,
                        BatchEndDateSnapshot = candidate.Course.BatchEndDate,

                        DeliveryModeSnapshot = candidate.DeliveryMode,

                        GeneratedAt = issueDate,
                        GeneratedByUserId = generatedByUserId
                    };

                    context.Certificates.Add(certificate);

                    generatedItems.Add(new CertificateGenerationItemDto
                    {
                        CertificateRequestId = candidate.Request.Id,
                        StudentId = candidate.Student.Id,
                        StudentName = candidate.Student.Name,
                        CourseId = candidate.Course.Id,
                        CourseTitle = candidate.Course.Title,
                        DeliveryMode = candidate.DeliveryMode,
                        CertificateNumber = certificateNumber
                    });
                }

                await context.SaveChangesAsync();

                var generatedRequestIds = generatedItems
                    .Select(x => x.CertificateRequestId)
                    .ToList();

                var savedCertificates = await context.Certificates
                    .AsNoTracking()
                    .Where(x =>
                        x.CourseId == courseId &&
                        generatedRequestIds.Contains(x.CertificateRequestId))
                    .Select(x => new
                    {
                        x.Id,
                        x.CertificateRequestId
                    })
                    .ToListAsync();

                foreach (var item in generatedItems)
                {
                    var saved = savedCertificates
                        .FirstOrDefault(x => x.CertificateRequestId == item.CertificateRequestId);

                    if (saved != null)
                    {
                        item.CertificateRecordId = saved.Id;
                    }
                }

                await transaction.CommitAsync();

                return new CertificateGenerationResultDto
                {
                    Success = true,
                    Message = $"{generatedItems.Count} certificate record(s) generated successfully.",
                    GeneratedCount = generatedItems.Count,
                    SkippedCount = skippedCount,
                    GeneratedCertificates = generatedItems
                };
            }
            catch
            {
                await transaction.RollbackAsync();

                return Fail("Certificate generation failed. Please try again.");
            }
        }

        private async Task<StudentCertificateNumber> GetOrCreateStudentCertificateNumberAsync(
            int studentId,
            string deliveryMode,
            int assignedByUserId,
            Dictionary<string, int> nextBaseNumberTracker)
        {
            var existing = await context.StudentCertificateNumbers
                .FirstOrDefaultAsync(x =>
                    x.StudentId == studentId &&
                    x.DeliveryMode == deliveryMode);

            if (existing != null)
                return existing;

            var nextBaseNumber = await GetNextBaseNumberAsync(
                deliveryMode,
                nextBaseNumberTracker);

            var studentCertificateNumber = new StudentCertificateNumber
            {
                StudentId = studentId,
                DeliveryMode = deliveryMode,
                BaseNumber = nextBaseNumber,
                AssignedAt = DateTime.UtcNow,
                AssignedByUserId = assignedByUserId
            };

            context.StudentCertificateNumbers.Add(studentCertificateNumber);

            return studentCertificateNumber;
        }

        private async Task<int> GetNextBaseNumberAsync(
            string deliveryMode,
            Dictionary<string, int> nextBaseNumberTracker)
        {
            if (!nextBaseNumberTracker.TryGetValue(deliveryMode, out var currentMax))
            {
                currentMax = await context.StudentCertificateNumbers
                    .Where(x => x.DeliveryMode == deliveryMode)
                    .MaxAsync(x => (int?)x.BaseNumber) ?? 0;
            }

            currentMax++;
            nextBaseNumberTracker[deliveryMode] = currentMax;

            return currentMax;
        }

        private static CertificateGenerationResultDto Fail(string message)
        {
            return new CertificateGenerationResultDto
            {
                Success = false,
                Message = message
            };
        }

        private class CertificateGenerationCandidate
        {
            public CertificateRequest Request { get; set; } = null!;
            public User Student { get; set; } = null!;
            public Course Course { get; set; } = null!;
            public CourseEnrollment Enrollment { get; set; } = null!;
            public string DeliveryMode { get; set; } = "";
        }
    }
}