using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminCoursesController : Controller
    {
        public IActionResult AdminMyCourses()
        {
            return View();
        }
        [HttpGet]
        public IActionResult AddNewCourse()
        {
            return View();
        }
    }
}
