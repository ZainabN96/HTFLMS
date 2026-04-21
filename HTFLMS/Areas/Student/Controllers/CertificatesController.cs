using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    public class CertificatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewCertificate(int id)
        {
            return View();
        }
    }
}