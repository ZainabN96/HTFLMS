using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class PricingsController : Controller
    {
        public IActionResult PricingsIndex()
        {
            return View();
        }
    }
}
