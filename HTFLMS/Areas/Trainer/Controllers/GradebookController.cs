using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    public class GradebookController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}