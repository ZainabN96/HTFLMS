using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult EnrollDetail(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        public IActionResult Details(int id = 1)
        {
            ViewBag.CourseId = id;
            return View();
        }
    }
}