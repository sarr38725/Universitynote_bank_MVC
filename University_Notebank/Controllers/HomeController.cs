using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using University_Notebank.Models;

namespace University_Notebank.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // ✅ Already logged in → go to Notes
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                if (HttpContext.Session.GetString("UserRole") == "Admin")
                    return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}