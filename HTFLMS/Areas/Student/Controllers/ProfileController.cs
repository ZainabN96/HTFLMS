using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}