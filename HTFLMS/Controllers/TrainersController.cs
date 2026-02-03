using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers
{
    public class TrainersController : Controller
    {
        public IActionResult TrainersIndex()
        {
            return View();
        }
    }
}
