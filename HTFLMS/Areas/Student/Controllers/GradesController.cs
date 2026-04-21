using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class GradesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}