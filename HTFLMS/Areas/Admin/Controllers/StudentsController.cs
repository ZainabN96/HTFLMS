using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StudentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.StudentId = id;
            return View();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}