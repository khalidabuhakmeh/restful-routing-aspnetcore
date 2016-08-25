using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Linq;

namespace RestfulRouting.Sample.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            var routes = RouteData
                .Routers
                .Where(x => x is RouteCollection)
                .Cast<RouteCollection>()
                .FirstOrDefault();

            return View(routes);
        }
    }
}
