using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class StudentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.StudentId = 0;
            return View("AddEditStudent");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.StudentId = id;
            return View("AddEditStudent");
        }
    }
}