using HTFLMS.Dtos.CertificateReview;

namespace HTFLMS.Data.IServices
{
    public interface ITrainerCertificateService
    {
        Task<CertificateReviewListDto?> GetReviewAsync(
            int trainerId,
            int? courseId,
            string? search,
            string? certificateStatus);

        Task<CertificateReviewActionResultDto> ApproveRequestAsync(
            int trainerId,
            int certificateRequestId);

        Task<CertificateReviewActionResultDto> RejectRequestAsync(
            int trainerId,
            int certificateRequestId);
    }
}