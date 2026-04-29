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

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.CourseId = id;
            return View("Edit");
        }
    }
}