using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminModulesLessonsController : Controller
    {
        public IActionResult AdminModules()
        {
            return View();
        }
        // =========================
        // CREATE MODULE
        // =========================
        [HttpGet]
        public IActionResult AddModule(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View("AddModule");
        }
    }

}
