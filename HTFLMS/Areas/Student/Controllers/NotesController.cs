using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class NotesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
