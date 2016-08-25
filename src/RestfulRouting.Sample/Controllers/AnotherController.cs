using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RestfulRouting.Sample.Controllers
{
    public class AnotherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult New()
        {
            return View();
        }

        public IActionResult Edit()
        {
            return View();
        }

        public IActionResult Create()
        {
            return RedirectToAction("new");
        }

        public IActionResult Update()
        {
            TempData["message"] = "We did a put!";
            return RedirectToAction("edit");
        }
    }
}
