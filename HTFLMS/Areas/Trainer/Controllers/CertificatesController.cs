using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Trainer.Controllers
{
    [Authorize(Roles = "Trainer")]
    [Area("Trainer")]
    public class CertificatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}