using HTFLMS.Dtos.CertificateGeneration;

namespace HTFLMS.Data.IServices
{
    public interface ICertificateGenerationService
    {
        Task<CertificateGenerationResultDto> GenerateForCourseAsync(
            int generatedByUserId,
            int courseId);
    }
}