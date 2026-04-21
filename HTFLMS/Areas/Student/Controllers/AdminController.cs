using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

<<<<<<<< HEAD:HTFLMS/Areas/Student/Controllers/AdminController.cs
namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
========
namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
>>>>>>>> 99efe061508fdc1df72004c34f13d2c1746e2e93:HTFLMS/Areas/Student/Controllers/StudentController.cs
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
