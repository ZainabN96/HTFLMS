using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id = 1)
        {
            ViewBag.CourseId = id;

            switch (id)
            {
                case 1:
                    ViewBag.CourseTitle = "Full Stack Web Development";
                    ViewBag.InstructorName = "John Doe";
                    ViewBag.CourseCategory = "Development";
                    ViewBag.CourseImage = Url.Content("~/img/course/course-1.webp");
                    ViewBag.CourseDescription = "Learn modern web development from fundamentals to building complete frontend and backend applications.";
                    break;

                case 2:
                    ViewBag.CourseTitle = "React Advanced Patterns";
                    ViewBag.InstructorName = "Jane Smith";
                    ViewBag.CourseCategory = "Frontend";
                    ViewBag.CourseImage = Url.Content("~/img/course/course-1.webp");
                    ViewBag.CourseDescription = "Explore reusable React patterns, state handling, composition, and scalable frontend architecture.";
                    break;

                case 3:
                    ViewBag.CourseTitle = "Node.js Backend Development";
                    ViewBag.InstructorName = "Mike Johnson";
                    ViewBag.CourseCategory = "Backend";
                    ViewBag.CourseImage = Url.Content("~/img/course/course-1.webp");
                    ViewBag.CourseDescription = "Build strong backend fundamentals with Node.js, APIs, routing, middleware, and database-ready server logic.";
                    break;

                case 4:
                    ViewBag.CourseTitle = "Python for Data Science";
                    ViewBag.InstructorName = "Sarah Lee";
                    ViewBag.CourseCategory = "Data Science";
                    ViewBag.CourseImage = Url.Content("~/img/course/course-1.webp");
                    ViewBag.CourseDescription = "Start with Python basics and move into data analysis, scripting, and practical data science workflows.";
                    break;

                default:
                    ViewBag.CourseTitle = "Full Stack Web Development";
                    ViewBag.InstructorName = "John Doe";
                    ViewBag.CourseCategory = "Development";
                    ViewBag.CourseImage = Url.Content("~/img/course/course-1.webp");
                    ViewBag.CourseDescription = "Learn modern web development from fundamentals to building complete frontend and backend applications.";
                    break;
            }

            return View();
        }
    }
}