using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class DropdownController : Controller
    {
        public IActionResult DropdownIndex()
        {
            return View();
        }
    }
}
