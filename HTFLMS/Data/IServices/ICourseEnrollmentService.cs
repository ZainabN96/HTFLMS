using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface ICourseEnrollmentService
    {
        Task<bool> IsAlreadyEnrolledAsync(
            int studentId,
            int courseId);

        Task<bool> HasAnyActiveEnrollmentAsync(
            int studentId);

        Task<List<int>> GetActiveStudentIdsByCourseAsync(
            int courseId);

        void Add(
            CourseEnrollment enrollment);
    }
}