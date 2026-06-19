using HTFLMS.Data.IServices;
using HTFLMS.Dtos.TrainerAssignmentGrading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HTFLMS.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Trainer")]
    public class TrainerAssignmentGradingController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public TrainerAssignmentGradingController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet("submissions")]
        public async Task<IActionResult> GetSubmissions(
            [FromQuery] string? search,
            [FromQuery] int? courseId,
            [FromQuery] int? moduleId,
            [FromQuery] string? status)
        {
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result = await uow.TrainerAssignmentGradingService.GetSubmissionsAsync(
                trainerId,
                search,
                courseId,
                moduleId,
                status
            );

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("submission/{submissionId:int}")]
        public async Task<IActionResult> GetSubmissionDetail(int submissionId)
        {
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            var result = await uow.TrainerAssignmentGradingService.GetSubmissionDetailAsync(
                trainerId,
                submissionId
            );

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Submission not found or you are not allowed to access it."
                });
            }

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpPost("submission/{submissionId:int}/grade")]
        public async Task<IActionResult> SaveGrade(
            int submissionId,
            [FromBody] TrainerAssignmentGradingSaveDto dto)
        {
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid grade data."
                });
            }

            try
            {
                var result = await uow.TrainerAssignmentGradingService.SaveGradeAsync(
                    trainerId,
                    submissionId,
                    dto
                );

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Submission not found or you are not allowed to grade it."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Unable to save grade. Please try again."
                });
            }
        }

        [HttpPost("missing/mark-zero")]
        public async Task<IActionResult> MarkMissingSubmissionZero(
            [FromBody] TrainerAssignmentGradingMarkZeroDto dto)
        {
            var trainerId = GetLoggedInTrainerId();

            if (trainerId <= 0)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Trainer login not found."
                });
            }

            if (dto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid missing submission data."
                });
            }

            try
            {
                var result = await uow.TrainerAssignmentGradingService.MarkMissingSubmissionZeroAsync(
                    trainerId,
                    dto
                );

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Assignment/student record not found or you are not allowed to update it."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    message = result.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Unable to mark missing submission as 0. Please try again."
                });
            }
        }

        private int GetLoggedInTrainerId()
        {
            var trainerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(trainerIdClaim))
            {
                return 0;
            }

            return int.TryParse(trainerIdClaim, out var trainerId)
                ? trainerId
                : 0;
        }
    }
}