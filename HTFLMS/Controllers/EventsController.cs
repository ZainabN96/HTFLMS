using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class EventsController : Controller
    {
        public IActionResult EventsIndex()
        {
            return View();
        }
    }
}
