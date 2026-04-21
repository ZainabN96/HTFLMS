using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class SlidesAssignmentsController : Controller
    {
        public IActionResult Index(int courseId = 1, int moduleId = 1)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            return View();
        }

        [HttpGet]
        public IActionResult CreateMaterial(int courseId = 1, int moduleId = 1)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            return View("CreateMaterial");
        }

        [HttpGet]
        public IActionResult CreateAssignment(int courseId = 1, int moduleId = 1)
        {
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            return View("CreateAssignment");
        }

        [HttpGet]
        public IActionResult EditMaterial(int id, int courseId = 1, int moduleId = 1)
        {
            ViewBag.Id = id;
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            return View();
        }

        [HttpGet]
        public IActionResult EditAssignment(int id, int courseId = 1, int moduleId = 1)
        {
            ViewBag.Id = id;
            ViewBag.CourseId = courseId;
            ViewBag.ModuleId = moduleId;
            return View();
        }
    }
}