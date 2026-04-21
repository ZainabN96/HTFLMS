using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ModulesLessonsController : Controller
    {
        // =========================
        // INDEX
        // =========================
        public IActionResult Index(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        // =========================
        // CREATE MODULE
        // =========================
        [HttpGet]
        public IActionResult CreateModule(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View("CreateModule");
        }

        [HttpPost]
        public IActionResult CreateModule(int courseId, string title, string description, int displayOrder, string status)
        {
            // Save module here

            return RedirectToAction("Index", new { id = courseId });
        }

        // =========================
        // EDIT MODULE
        // =========================
        [HttpGet]
        public IActionResult EditModule(int courseId, int moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;

            return View("EditModule");
        }

        [HttpPost]
        public IActionResult EditModule(int courseId, int moduleId, string title, string description, int displayOrder, string status)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;

            return RedirectToAction("Index", new { id = courseId });
        }

        // =========================
        // CREATE LESSON
        // =========================
        [HttpGet]
        public IActionResult CreateLesson(int courseId, int? moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;

            return View("CreateLesson");
        }

        [HttpPost]
        public IActionResult CreateLesson(
            int courseId,
            int moduleId,
            string title,
            string description,
            int displayOrder,
            string duration,
            string access,
            string content)
        {
            // Save lesson here

            return RedirectToAction("Index", new { id = courseId });
        }

        // =========================
        // EDIT LESSON
        // =========================
        [HttpGet]
        public IActionResult EditLesson(int courseId, int lessonId, int? moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.LessonId = lessonId;
            ViewBag.ModuleId = moduleId;

            return View("EditLesson");
        }

        [HttpPost]
        public IActionResult EditLesson(
            int courseId,
            int lessonId,
            int moduleId,
            string title,
            string description,
            int displayOrder,
            string duration,
            string access,
            string content)
        {
            ViewBag.CourseId = courseId;
            ViewBag.LessonId = lessonId;
            ViewBag.ModuleId = moduleId;

            return RedirectToAction("Index", new { id = courseId });
        }

        // =========================
        // CREATE QUIZ
        // =========================
        [HttpGet]
        public IActionResult CreateQuiz(int courseId, int? moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;

            return View("CreateQuiz");
        }

        [HttpPost]
        public IActionResult CreateQuiz(
            int courseId,
            int moduleId,
            string title,
            string description,
            int displayOrder,
            int attemptsAllowed,
            int passingMarks,
            string instructions)
        {
            // Save quiz here

            return RedirectToAction("Index", new { id = courseId });
        }

        // =========================
        // EDIT QUIZ
        // =========================
        [HttpGet]
        public IActionResult EditQuiz(int courseId, int quizId, int? moduleId)
        {
            ViewBag.CourseId = courseId;
            ViewBag.QuizId = quizId;
            ViewBag.ModuleId = moduleId;

            return View("EditQuiz");
        }

        [HttpPost]
        public IActionResult EditQuiz(
            int courseId,
            int quizId,
            int moduleId,
            string title,
            string description,
            int displayOrder,
            int attemptsAllowed,
            int passingMarks,
            string instructions)
        {
            ViewBag.CourseId = courseId;
            ViewBag.QuizId = quizId;
            ViewBag.ModuleId = moduleId;

            return RedirectToAction("Index", new { id = courseId });
        }
    }
}