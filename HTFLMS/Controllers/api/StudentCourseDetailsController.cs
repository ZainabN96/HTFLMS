using HTFLMS.Data.IServices;
using HTFLMS.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Route("api/student/course-details")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class StudentCourseDetailsController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public StudentCourseDetailsController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet("{courseId:int}")]
        public async Task<IActionResult> GetStudentCourseDetails(int courseId)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "Student is not logged in."));

            var student = await uow.UserService.GetUserByEmailAsync(email);

            if (student == null)
                return NotFound(new APIError(404, "Student account was not found."));

            var isEnrolled = await uow.CourseEnrollmentService
                .IsAlreadyEnrolledAsync(student.Id, courseId);

            if (!isEnrolled)
                return Unauthorized(new APIError(401, "You are not enrolled in this course."));

            var course = await uow.CourseService.GetByIdAsync(courseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            var users = await uow.UserService.GetAllAsync();
            var trainer = users.FirstOrDefault(x => x.Id == course.TrainerId);

            var modules = await uow.ModuleService.GetByCourseIdAsync(courseId);

            var moduleResult = new List<object>();

            foreach (var module in modules.Where(m => m.IsActive).OrderBy(m => m.DisplayOrder))
            {
                var lessons = await uow.LessonService.GetByModuleIdAsync(module.Id);
                var quizzes = await uow.QuizService.GetByModuleIdAsync(module.Id);

                var quizResult = new List<object>();

                foreach (var quizItem in quizzes.Where(q => q.IsActive && q.IsAccessible))
                {
                    var fullQuiz = await uow.QuizService.GetByIdAsync(quizItem.Id);

                    if (fullQuiz == null)
                        continue;

                    quizResult.Add(new
                    {
                        fullQuiz.Id,
                        fullQuiz.Title,
                        fullQuiz.Instructions,
                        fullQuiz.AttemptsAllowed,
                        questionsCount = fullQuiz.Questions == null ? 0 : fullQuiz.Questions.Count,

                        questions = fullQuiz.Questions == null
                            ? new List<object>()
                            : fullQuiz.Questions
                                .OrderBy(q => q.DisplayOrder)
                                .Select(q => new
                                {
                                    q.Id,
                                    q.QuestionText,
                                    q.DisplayOrder,

                                    options = q.Options == null
                                        ? new List<object>()
                                        : q.Options.Select(o => new
                                        {
                                            o.Id,
                                            o.OptionText,
                                            o.IsCorrect
                                        }).ToList<object>()
                                }).ToList<object>()
                    });
                }
                var materials = await uow.MaterialService.GetByModuleIdAsync(module.Id);
                var assignments = await uow.AssignmentService.GetByModuleIdAsync(module.Id);

                moduleResult.Add(new
                {
                    module.Id,
                    module.Title,
                    module.Description,
                    module.DisplayOrder,
                    module.IsAccessible,

                    lessons = lessons
                        .Where(l => l.IsActive)
                        .OrderBy(l => l.DisplayOrder)
                        .Select(l => new
                        {
                            l.Id,
                            l.Title,
                            l.Description,
                            l.DisplayOrder
                        })
                        .ToList(),

                    quizzes = quizzes
                        .Where(q => q.IsActive && q.IsAccessible)
                        .Select(q => new
                        {
                            q.Id,
                            q.Title,
                            q.Instructions,
                            q.AttemptsAllowed,
                            questionsCount = q.Questions == null ? 0 : q.Questions.Count,

                            questions = q.Questions == null
                                ? new List<object>()
                                : q.Questions
                                    .OrderBy(x => x.DisplayOrder)
                                    .Select(question => new
                                    {
                                        question.Id,
                                        question.QuestionText,
                                        question.DisplayOrder,

                                        options = question.Options == null
                                            ? new List<object>()
                                            : question.Options.Select(option => new
                                            {
                                                option.Id,
                                                option.OptionText,
                                                option.IsCorrect
                                            }).ToList<object>()
                                    }).ToList<object>()
                        })
                        .ToList(),

                    materials = materials
                        .Where(m => m.IsActive)
                        .Select(m => new
                        {
                            m.Id,
                            m.Title,
                            m.ContentType,
                            m.FilePath,
                            m.ExternalUrl,
                            m.Pages,
                            m.Slides,
                            m.Minutes
                        })
                        .ToList(),

                    assignments = assignments
                        .Where(a => a.IsActive)
                        .Select(a => new
                        {
                            a.Id,
                            a.Title,
                            a.Description,
                            a.Marks,
                            a.DueDateTime,
                            a.FilePath
                        })
                        .ToList()
                });
            }

            return Ok(new
            {
                courseId = course.Id,
                course.Title,
                course.Category,
                course.Description,
                course.CourseImagePath,
                course.CertificateIncluded,
                course.BatchStartDate,
                course.BatchEndDate,
                trainerName = trainer == null ? "No Trainer" : trainer.Name,
                progressPercentage = 0,
                modules = moduleResult
            });
        }
    }
}