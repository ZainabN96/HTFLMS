using HTFLMS.Dtos.StudentCertificate;

namespace HTFLMS.Data.IServices
{
    public interface IStudentCertificateService
    {
        Task<List<StudentCertificateDto>> GetCertificatesAsync(int studentId);
        Task<StudentCertificateApplyResultDto> ApplyAsync(int studentId, int courseId);
        Task<StudentCertificateDetailDto?> GetCertificateDetailAsync(int studentId, int certificateRequestId);
    }
}