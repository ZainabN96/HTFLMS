using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace HTFLMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        public IActionResult Create(
            string title,
            string category,
            string level,
            string instructor,
            string courseCode,
            string description,
            string content,
            IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Save image here later
                // Example:
                // var fileName = Path.GetFileName(imageFile.FileName);
                // var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/courses", fileName);
                // using var stream = new FileStream(filePath, FileMode.Create);
                // imageFile.CopyTo(stream);
            }

            // Save course here

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewBag.CourseId = id;

            // For preview of existing image
            ViewBag.ImageUrl = "/uploads/courses/course-placeholder.png";

            return View("Edit");
        }

        [HttpPost]
        public IActionResult Edit(
            int id,
            string title,
            string category,
            string level,
            string instructor,
            string courseCode,
            string description,
            string content,
            IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Replace image here later
            }

            // Update course here

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            ViewBag.CourseId = id;
            return View("Delete");
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            // Delete course here
            return RedirectToAction("Index");
        }
    }
}