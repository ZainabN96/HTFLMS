using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            ViewBag.CourseId = null;
            return View("AddEditCourse");
        }

        public IActionResult Edit(int id)
        {
            ViewBag.CourseId = id;
            return View("AddEditCourse");
        }
    }
}