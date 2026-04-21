using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class GradebookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}