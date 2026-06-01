using HTFLMS.Data.IServices;
using HTFLMS.Dtos;
using HTFLMS.Errors;
using HTFLMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public QuizController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] QuizDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var module = await uow.ModuleService.GetByIdAsync(dto.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to add quizzes to this module.")
                );
            }

            var quiz = new Quiz
            {
                ModuleId = dto.ModuleId,
                Title = dto.Title,
                Instructions = dto.Instructions,
                AttemptsAllowed = dto.AttemptsAllowed,
                IsActive = dto.IsActive,
                IsAccessible = dto.IsAccessible,
                CreatedAt = DateTime.UtcNow,
                Questions = new List<QuizQuestion>()
            };

            int questionOrder = 1;

            foreach (var q in dto.Questions)
            {
                var question = new QuizQuestion
                {
                    QuestionText = q.QuestionText,
                    DisplayOrder = q.DisplayOrder > 0 ? q.DisplayOrder : questionOrder,
                    Options = new List<QuizOption>
                    {
                        new QuizOption { OptionText = q.OptionA, IsCorrect = q.CorrectAnswer == "A" },
                        new QuizOption { OptionText = q.OptionB, IsCorrect = q.CorrectAnswer == "B" },
                        new QuizOption { OptionText = q.OptionC, IsCorrect = q.CorrectAnswer == "C" },
                        new QuizOption { OptionText = q.OptionD, IsCorrect = q.CorrectAnswer == "D" }
                    }
                };

                quiz.Questions.Add(question);
                questionOrder++;
            }

            uow.QuizService.Add(quiz);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Quiz created successfully.",
                quizId = quiz.Id
            });
        }

        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetQuizzesByModule(int moduleId)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var module = await uow.ModuleService.GetByIdAsync(moduleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to view these quizzes.")
                );
            }

            var quizzes = await uow.QuizService.GetByModuleIdAsync(moduleId);

            var result = quizzes.Select(q => new
            {
                q.Id,
                q.ModuleId,
                q.Title,
                q.Instructions,
                q.AttemptsAllowed,
                q.IsActive,
                q.IsAccessible,
                q.CreatedAt,
                QuestionsCount = q.Questions == null ? 0 : q.Questions.Count
            });

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetQuiz(int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var quiz = await uow.QuizService.GetByIdAsync(id);

            if (quiz == null)
                return NotFound(new APIError(404, "Quiz not found."));

            var module = await uow.ModuleService.GetByIdAsync(quiz.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to view this quiz.")
                );
            }

            return Ok(new
            {
                quiz.Id,
                quiz.ModuleId,
                quiz.Title,
                quiz.Instructions,
                quiz.AttemptsAllowed,
                quiz.IsActive,
                quiz.IsAccessible,
                Questions = quiz.Questions == null
                    ? new List<object>()
                    : quiz.Questions
                        .OrderBy(q => q.DisplayOrder)
                        .Select(q => new
                        {
                            q.Id,
                            q.QuestionText,
                            q.DisplayOrder,
                            Options = q.Options == null
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

        [HttpPut("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] QuizDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var quiz = await uow.QuizService.GetByIdAsync(id);

            if (quiz == null)
                return NotFound(new APIError(404, "Quiz not found."));

            var module = await uow.ModuleService.GetByIdAsync(dto.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to edit this quiz.")
                );
            }

            quiz.ModuleId = dto.ModuleId;
            quiz.Title = dto.Title;
            quiz.Instructions = dto.Instructions;
            quiz.AttemptsAllowed = dto.AttemptsAllowed;
            quiz.IsActive = dto.IsActive;
            quiz.IsAccessible = dto.IsAccessible;

            quiz.Questions?.Clear();
            quiz.Questions = new List<QuizQuestion>();

            int questionOrder = 1;

            foreach (var q in dto.Questions)
            {
                var question = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionText = q.QuestionText,
                    DisplayOrder = q.DisplayOrder > 0 ? q.DisplayOrder : questionOrder,
                    Options = new List<QuizOption>
                    {
                        new QuizOption { OptionText = q.OptionA, IsCorrect = q.CorrectAnswer == "A" },
                        new QuizOption { OptionText = q.OptionB, IsCorrect = q.CorrectAnswer == "B" },
                        new QuizOption { OptionText = q.OptionC, IsCorrect = q.CorrectAnswer == "C" },
                        new QuizOption { OptionText = q.OptionD, IsCorrect = q.CorrectAnswer == "D" }
                    }
                };

                quiz.Questions.Add(question);
                questionOrder++;
            }

            uow.QuizService.Update(quiz);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Quiz updated successfully.",
                quizId = quiz.Id
            });
        }

        [HttpPut("toggle-access/{id:int}")]
        public async Task<IActionResult> ToggleAccess(int id, [FromBody] ToggleQuizAccessDto dto)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var quiz = await uow.QuizService.GetByIdAsync(id);

            if (quiz == null)
                return NotFound(new APIError(404, "Quiz not found."));

            var module = await uow.ModuleService.GetByIdAsync(quiz.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to update this quiz access.")
                );
            }

            quiz.IsAccessible = dto.IsAccessible;

            uow.QuizService.Update(quiz);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Quiz access updated successfully."
            });
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new APIError(401, "User is not logged in."));

            var user = await uow.UserService.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound(new APIError(404, "Logged-in user was not found."));

            var quiz = await uow.QuizService.GetByIdAsync(id);

            if (quiz == null)
                return NotFound(new APIError(404, "Quiz not found."));

            var module = await uow.ModuleService.GetByIdAsync(quiz.ModuleId);

            if (module == null)
                return NotFound(new APIError(404, "Module not found."));

            var course = await uow.CourseService.GetByIdAsync(module.CourseId);

            if (course == null)
                return NotFound(new APIError(404, "Course not found."));

            if (!IsAdmin() && course.TrainerId != user.Id)
            {
                return Unauthorized(
                    new APIError(401, "You are not allowed to delete this quiz.")
                );
            }

            uow.QuizService.Delete(quiz);

            await uow.SaveAsync();

            return Ok(new
            {
                message = "Quiz deleted successfully."
            });
        }
    }
}