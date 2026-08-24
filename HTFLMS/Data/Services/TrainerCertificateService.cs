using HTFLMS.Data.IServices;
using HTFLMS.Dtos.CertificateReview;
using HTFLMS.Helper;
using Microsoft.EntityFrameworkCore;
using HTFLMS.Dtos.CertificateGeneration;

namespace HTFLMS.Data.Services
{
    public class TrainerCertificateService : ITrainerCertificateService
    {
        private readonly ApplicationDbContext context;
        private readonly CertificateReviewHelper certificateReviewHelper;
        private readonly CertificateGenerationService certificateGenerationService;

        public TrainerCertificateService(ApplicationDbContext context)
        {
            this.context = context;
            certificateReviewHelper = new CertificateReviewHelper(context);
            certificateGenerationService = new CertificateGenerationService(context);
        }

        public async Task<CertificateReviewListDto?> GetReviewAsync(
            int trainerId,
            int? courseId,
            string? search,
            string? certificateStatus)
        {
            var courseOptions = await context.Courses
                .AsNoTracking()
                .Where(x =>
                    x.TrainerId == trainerId &&
                    x.IsActive &&
                    x.CertificateIncluded)
                .OrderBy(x => x.Title)
                .Select(x => new CertificateReviewCourseOptionDto
                {
                    CourseId = x.Id,
                    CourseTitle = x.Title
                })
                .ToListAsync();

            if (!courseOptions.Any())
            {
                return new CertificateReviewListDto
                {
                    Filters = new CertificateReviewFilterDto
                    {
                        Courses = courseOptions
                    },
                    EmptyMessage = "No certificate courses found."
                };
            }

            var selectedCourseId = courseId.HasValue && courseId.Value > 0
                ? courseId.Value
                : courseOptions.First().CourseId;

            var isAllowedCourse = courseOptions.Any(x => x.CourseId == selectedCourseId);

            if (!isAllowedCourse)
            {
                return null;
            }

            return await certificateReviewHelper.BuildReviewAsync(
                selectedCourseId,
                search,
                certificateStatus,
                courseOptions);
        }

        public async Task<CertificateReviewActionResultDto> ApproveRequestAsync(
            int trainerId,
            int certificateRequestId)
        {
            var isAllowed = await IsTrainerAllowedForRequestAsync(
                trainerId,
                certificateRequestId);

            if (!isAllowed)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Certificate request was not found or you are not allowed to approve it."
                };
            }

            return await certificateReviewHelper.ApproveRequestAsync(
                trainerId,
                certificateRequestId);
        }

        public async Task<CertificateReviewActionResultDto> RejectRequestAsync(
            int trainerId,
            int certificateRequestId)
        {
            var isAllowed = await IsTrainerAllowedForRequestAsync(
                trainerId,
                certificateRequestId);

            if (!isAllowed)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Certificate request was not found or you are not allowed to reject it."
                };
            }

            return await certificateReviewHelper.RejectRequestAsync(
                trainerId,
                certificateRequestId);
        }

        public async Task<CertificateReviewActionResultDto> UpdateDeliveryModeAsync(
            int trainerId,
            int enrollmentId,
            string deliveryMode)
        {
            var isAllowed = await IsTrainerAllowedForEnrollmentAsync(
                trainerId,
                enrollmentId);

            if (!isAllowed)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Enrollment record was not found or you are not allowed to update it."
                };
            }


            return await certificateReviewHelper.UpdateDeliveryModeAsync(
                trainerId,
                enrollmentId,
                deliveryMode);
        }
        public async Task<CertificateGenerationResultDto> GenerateCertificatesAsync(
    int trainerId,
    int courseId)
        {
            var isAllowed = await context.Courses
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == courseId &&
                    x.TrainerId == trainerId &&
                    x.IsActive &&
                    x.CertificateIncluded);

            if (!isAllowed)
            {
                return new CertificateGenerationResultDto
                {
                    Success = false,
                    Message = "Course was not found or you are not allowed to generate certificates for this course."
                };
            }

            return await certificateGenerationService.GenerateForCourseAsync(
                trainerId,
                courseId);
        }
        private async Task<bool> IsTrainerAllowedForRequestAsync(
            int trainerId,
            int certificateRequestId)
        {
            return await context.CertificateRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == certificateRequestId &&
                    x.Course != null &&
                    x.Course.TrainerId == trainerId &&
                    x.Course.IsActive &&
                    x.Course.CertificateIncluded);
        }

        private async Task<bool> IsTrainerAllowedForEnrollmentAsync(
            int trainerId,
            int enrollmentId)
        {
            return await context.CourseEnrollments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == enrollmentId &&
                    x.Status == "Active" &&
                    x.Course != null &&
                    x.Course.TrainerId == trainerId &&
                    x.Course.IsActive &&
                    x.Course.CertificateIncluded);
        }
    }
}