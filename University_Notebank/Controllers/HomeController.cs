using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using University_Notebank.Models;

namespace University_Notebank.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                if (HttpContext.Session.GetString("UserRole") == "Admin")
                    return RedirectToAction("Index", "Admin");
            }

            var recentNotes = _db.Notes
                .Include(n => n.Major)
                .Include(n => n.Term)
                .Where(n => n.Status == "Approved")
                .OrderByDescending(n => n.CreatedAt)
                .Take(4)
                .ToList();

            ViewBag.RecentNotes = recentNotes;
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
