using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class SlidesAssignmentsController : Controller
    {
        public IActionResult Index(int courseId = 0, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;

            return View();
        }

        [HttpGet]
        public IActionResult CreateMaterial(int courseId, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.LessonId = 0;
            ViewBag.MaterialId = 0;

            return View("AddEditMaterial");
        }

        [HttpGet]
        public IActionResult EditMaterial(int id, int courseId, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.LessonId = 0;
            ViewBag.MaterialId = id;

            return View("AddEditMaterial");
        }

        [HttpGet]
        public IActionResult CreateAssignment(int courseId, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.AssignmentId = 0;

            return View("AddEditAssignment");
        }

        [HttpGet]
        public IActionResult EditAssignment(int id, int courseId, int moduleId = 0)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            ViewBag.AssignmentId = id;

            return View("AddEditAssignment");
        }
    }
}