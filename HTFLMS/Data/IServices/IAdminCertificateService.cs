using HTFLMS.Dtos.CertificateReview;

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
    }
}