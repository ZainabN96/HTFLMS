using HTFLMS.Dtos.CertificateReview;
using HTFLMS.Dtos.CertificateGeneration;

namespace HTFLMS.Data.IServices
{
    public interface IAdminCertificateService
    {
        Task<CertificateReviewListDto?> GetReviewAsync(
            int? courseId,
            string? search,
            string? certificateStatus);

        Task<CertificateReviewActionResultDto> ApproveRequestAsync(
            int adminId,
            int certificateRequestId);

        Task<CertificateReviewActionResultDto> RejectRequestAsync(
            int adminId,
            int certificateRequestId);

        Task<CertificateReviewActionResultDto> UpdateDeliveryModeAsync(
            int adminId,
            int enrollmentId,
            string deliveryMode);
        Task<CertificateGenerationResultDto> GenerateCertificatesAsync(
    int adminId,
    int courseId);
    }
}