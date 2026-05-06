using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoursesController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.CourseId = "";
            return View("Create");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.CourseId = id;
            return View("Create");
        }
    }
}