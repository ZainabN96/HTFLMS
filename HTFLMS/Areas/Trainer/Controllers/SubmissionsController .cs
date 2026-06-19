using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Area("Trainer")]
    [Authorize(Roles = "Trainer")]
    public class SubmissionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Grade(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SubmissionId = id;
            ViewBag.Mode = "grade";

            return View("Grade");
        }

        public IActionResult Edit(int id)
        {
            if (id <= 0)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SubmissionId = id;
            ViewBag.Mode = "edit";

            return View("Grade");
        }
    }
}