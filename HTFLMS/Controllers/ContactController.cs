using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult ContactIndex()
        {
            return View();
        }
        public IActionResult Form()
        {
            return View();
        }
    }
}
