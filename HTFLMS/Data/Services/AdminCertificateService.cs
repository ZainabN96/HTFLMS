using HTFLMS.Data.IServices;
using HTFLMS.Dtos.CertificateReview;
using HTFLMS.Helper;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class AdminCertificateService : IAdminCertificateService
    {
        private readonly ApplicationDbContext context;
        private readonly CertificateReviewHelper certificateReviewHelper;

        public AdminCertificateService(ApplicationDbContext context)
        {
            this.context = context;
            certificateReviewHelper = new CertificateReviewHelper(context);
        }

        public async Task<CertificateReviewListDto?> GetReviewAsync(
            int? courseId,
            string? search,
            string? certificateStatus)
        {
            var courseOptions = await context.Courses
                .AsNoTracking()
                .Where(x =>
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
            int adminId,
            int certificateRequestId)
        {
            var isAllowed = await IsAdminAllowedForRequestAsync(certificateRequestId);

            if (!isAllowed)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Certificate request was not found or this course is not available."
                };
            }

            return await certificateReviewHelper.ApproveRequestAsync(
                adminId,
                certificateRequestId);
        }

        public async Task<CertificateReviewActionResultDto> RejectRequestAsync(
            int adminId,
            int certificateRequestId)
        {
            var isAllowed = await IsAdminAllowedForRequestAsync(certificateRequestId);

            if (!isAllowed)
            {
                return new CertificateReviewActionResultDto
                {
                    Success = false,
                    Message = "Certificate request was not found or this course is not available."
                };
            }

            return await certificateReviewHelper.RejectRequestAsync(
                adminId,
                certificateRequestId);
        }

        private async Task<bool> IsAdminAllowedForRequestAsync(int certificateRequestId)
        {
            return await context.CertificateRequests
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == certificateRequestId &&
                    x.Course != null &&
                    x.Course.IsActive &&
                    x.Course.CertificateIncluded);
        }
    }
}