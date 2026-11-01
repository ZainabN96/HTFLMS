using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class GalleryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
