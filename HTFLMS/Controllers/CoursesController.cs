//using HTFLMS.Data;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace HTFLMS.Controllers
//{
//    public class CoursesController : Controller
//    {
//        private readonly ApplicationDbContext _db;

//        public CoursesController(ApplicationDbContext db)
//        {
//            _db = db;
//        }
//        public async Task<IActionResult> CoursesIndex()
//        {
//            var courses = await _db.Courses
//                .OrderByDescending(c => c.Id)
//                .ToListAsync();

//            return View(courses);
//        }
//        public IActionResult CourseHome()
//        {
//            return View();
//        }
//    }
//}
using HTFLMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CoursesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> CoursesIndex()
        {
            var courses = await _db.Courses
                .Include(c => c.Trainer)
                .Where(c => c.IsActive == true && c.IsPublished == true)
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return View(courses);
        }

        public IActionResult CourseHome()
        {
            return View();
        }
    }
}