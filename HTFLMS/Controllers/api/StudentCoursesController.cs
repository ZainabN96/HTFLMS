using HTFLMS.Data.IServices;
using HTFLMS.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Route("api/student/courses")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public StudentCoursesController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseDetail(int id)
        {
            var course = await uow.CourseService.GetStudentCourseDetailAsync(id);

            if (course == null)
            {
                return NotFound(new APIError(404, "Course not found or not available for enrollment."));
            }

            return Ok(course);
        }
    }
}