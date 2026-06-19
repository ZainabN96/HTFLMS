using HTFLMS.Dtos.TrainerAssignmentGrading;

namespace HTFLMS.Data.IServices
{
    public interface ITrainerAssignmentGradingService
    {
        Task<TrainerAssignmentGradingListDto> GetSubmissionsAsync(
            int trainerId,
            string? search,
            int? courseId,
            int? moduleId,
            string? status);

        Task<TrainerAssignmentGradingDetailDto?> GetSubmissionDetailAsync(
            int trainerId,
            int submissionId);

        Task<TrainerAssignmentGradingSaveResultDto?> SaveGradeAsync(
            int trainerId,
            int submissionId,
            TrainerAssignmentGradingSaveDto dto);

        Task<TrainerAssignmentGradingSaveResultDto?> MarkMissingSubmissionZeroAsync(
            int trainerId,
            TrainerAssignmentGradingMarkZeroDto dto);
    }
}