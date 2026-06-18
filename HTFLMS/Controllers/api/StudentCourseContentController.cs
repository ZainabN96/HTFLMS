using HTFLMS.Data.IServices;
using HTFLMS.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HTFLMS.Dtos.StudentCourseContent;

namespace HTFLMS.Controllers.api
{
    [Route("api/student/course-content")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentCourseContentController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public StudentCourseContentController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet("{courseId:int}/header")]
        public async Task<IActionResult> GetHeader(int courseId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.GetHeaderAsync(studentId.Value, courseId);

            if (result == null)
            {
                return NotFound(new APIError(404, "Course header was not found or you are not enrolled in this course."));
            }

            return Ok(result);
        }

        [HttpGet("{courseId:int}/info")]
        public async Task<IActionResult> GetInfo(int courseId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.GetInfoAsync(studentId.Value, courseId);

            if (result == null)
            {
                return NotFound(new APIError(404, "Course info was not found or you are not enrolled in this course."));
            }

            return Ok(result);
        }

        private int? GetStudentId()
        {
            var studentIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(studentIdClaim))
            {
                return null;
            }

            if (!int.TryParse(studentIdClaim, out var studentId))
            {
                return null;
            }

            return studentId;
        }
        [HttpGet("{courseId:int}/modules-lessons")]
        public async Task<IActionResult> GetModulesAndLessons(int courseId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.GetModulesAndLessonsAsync(studentId.Value, courseId);

            if (result == null)
            {
                return NotFound(new APIError(404, "Course modules were not found or you are not enrolled in this course."));
            }

            return Ok(result);
        }

        [HttpPost("lessons/{lessonId:int}/mark-done")]
        public async Task<IActionResult> MarkLessonDone(int lessonId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.MarkLessonDoneAsync(studentId.Value, lessonId);

            if (!result)
            {
                return BadRequest(new APIError(400, "Lesson could not be marked as completed."));
            }

            return Ok(new
            {
                success = true,
                message = "Lesson marked as completed."
            });
        }
        [HttpGet("modules/{moduleId:int}/quiz")]
        public async Task<IActionResult> GetQuiz(int moduleId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.GetQuizAsync(studentId.Value, moduleId);

            if (result == null)
            {
                return NotFound(new APIError(404, "Quiz was not found or is not accessible."));
            }

            return Ok(result);
        }

        [HttpPost("modules/{moduleId:int}/quiz/submit")]
        public async Task<IActionResult> SubmitQuiz(int moduleId, [FromBody] StudentCourseContentQuizSubmitDto dto)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            dto.ModuleId = moduleId;

            var result = await uow.StudentCourseContentService.SubmitQuizAsync(studentId.Value, dto);

            if (result == null)
            {
                return BadRequest(new APIError(400, "Quiz could not be submitted."));
            }

            return Ok(result);
        }

        [HttpGet("quizzes/{quizId:int}/review")]
        public async Task<IActionResult> GetQuizReview(int quizId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.GetQuizReviewAsync(studentId.Value, quizId);

            if (result == null)
            {
                return NotFound(new APIError(404, "Quiz review was not found."));
            }

            return Ok(result);
        }
        //////////////////Slides And Assignments///////////////

        [HttpGet("{courseId:int}/materials-assignments")]
        public async Task<IActionResult> GetMaterialsAndAssignments(int courseId)
        {
            var studentId = GetStudentId();

            if (studentId == null)
            {
                return Unauthorized(new APIError(401, "Student session is invalid. Please login again."));
            }

            var result = await uow.StudentCourseContentService.GetMaterialsAndAssignmentsAsync(studentId.Value, courseId);

            if (result == null)
            {
                return NotFound(new APIError(404, "Course materials and assignments were not found or you are not enrolled in this course."));
            }

            return Ok(result);
        }
    }
}