using Microsoft.AspNetCore.Mvc;

namespace RestfulRouting.Sample.Controllers
{
    public class PostsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        
    }
}