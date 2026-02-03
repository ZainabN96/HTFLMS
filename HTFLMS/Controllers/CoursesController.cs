using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult CoursesIndex()
        {
            return View();
        }
    }
}
