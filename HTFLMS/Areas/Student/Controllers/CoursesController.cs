using HTFLMS.Data.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CoursesController : Controller
    {
        private readonly IUnitOfWork uow;

        public CoursesController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Enroll()
        {
            var courses = await uow.CourseService.GetAllAsync();

            var activeCourses = courses
                .Where(c => c.IsActive)
                .ToList();

            return View(activeCourses);
        }

        public IActionResult EnrollDetail(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        public IActionResult Details(int id = 1)
        {
            ViewBag.CourseId = id;
            return View();
        }
    }
}