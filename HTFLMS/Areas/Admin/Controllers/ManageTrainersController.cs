using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ManageTrainersController : Controller
    {
        public IActionResult Trainers()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddTrainer()
        {
            ViewBag.TrainerId = 0;
            return View("AddEditTrainer");
        }

        [HttpGet]
        public IActionResult EditTrainer(int id)
        {
            ViewBag.TrainerId = id;
            return View("AddEditTrainer");
        }
    }
}