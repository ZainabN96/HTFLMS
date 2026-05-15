using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class ModulesLessonsController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.CourseId = 0;
            return View();
        }

        [HttpGet]
        public IActionResult CreateModule(int courseId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = 0;

            return View("AddEditModule");
        }

        [HttpGet]
        public IActionResult EditModule(int courseId, int moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;

            return View("AddEditModule");
        }

        [HttpGet]
        public IActionResult CreateLesson(int courseId, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.LessonId = 0;

            return View("AddEditLesson");
        }

        [HttpGet]
        public IActionResult EditLesson(int courseId, int moduleId, int lessonId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.LessonId = lessonId;

            return View("AddEditLesson");
        }

        [HttpGet]
        public IActionResult CreateQuiz(int courseId, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.QuizId = 0;

            return View("AddEditQuiz");
        }

        [HttpGet]
        public IActionResult EditQuiz(int courseId, int moduleId, int quizId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.QuizId = quizId;

            return View("AddEditQuiz");
        }
    }
}