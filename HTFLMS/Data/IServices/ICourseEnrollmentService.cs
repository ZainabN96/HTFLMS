using HTFLMS.Models;

namespace HTFLMS.Data.IServices
{
    public interface ICourseEnrollmentService
    {
        Task<bool> IsAlreadyEnrolledAsync(int studentId, int courseId);

        Task<bool> HasAnyActiveEnrollmentAsync(int studentId);

        void Add(CourseEnrollment enrollment);
    }
}