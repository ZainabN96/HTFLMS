using Microsoft.AspNetCore.Mvc;   

namespace HTFLMS.Controllers
{
   
    public class AboutController : Controller
    {
     
        public IActionResult AboutIndex()
        {
            return View();  
        }
    }
}

