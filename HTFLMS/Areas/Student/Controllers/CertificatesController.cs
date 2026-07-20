using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Authorize]
    [Area("Student")]
    public class CertificatesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewCertificate(int id)
        {
            ViewBag.CertificateRequestId = id;
            return View();
        }
    }
}




//using Microsoft.AspNetCore.Mvc;

//namespace HTFLMS.Areas.Student.Controllers
//{
//    [Area("Student")]
//    public class CertificatesController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }

//        public IActionResult ViewCertificate(int id)
//        {
//            return View();
//        }
//    }
//}