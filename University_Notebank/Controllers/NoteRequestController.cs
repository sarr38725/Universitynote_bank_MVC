using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using University_Notebank.Models;

namespace University_Notebank.Controllers
{
    public class NoteRequestController : Controller
    {
        private readonly AppDbContext _db;

        public NoteRequestController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetString("UserId") != null;
        private int CurrentUserId() => (int)HttpContext.Session.GetInt32("UserId");

        // GET: /NoteRequest/MyRequests
        public IActionResult MyRequests()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var requests = _db.NoteRequests
                .Include(r => r.Major)
                .Include(r => r.Term)
                .Include(r => r.Messages)
                .Where(r => r.UserId == CurrentUserId())
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return View(requests);
        }

        // GET: /NoteRequest/Create
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            ViewBag.Majors = new SelectList(_db.Majors.ToList(), "Id", "Name");
            ViewBag.Terms = new SelectList(_db.Terms.ToList(), "Id", "Name");
            ViewBag.Batches = Enumerable.Range(10, 21)
                .Select(b => new SelectListItem { Value = b.ToString(), Text = "Batch " + b })
                .ToList();

            return View();
        }

        // POST: /NoteRequest/Create
        [HttpPost]
        public IActionResult Create(NoteRequest model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            model.UserId = CurrentUserId();
            model.Status = "Pending";
            model.CreatedAt = DateTime.Now;

            _db.NoteRequests.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "আপনার request পাঠানো হয়েছে! Admin শীঘ্রই দেখবেন।";
            return RedirectToAction("MyRequests");
        }

        // GET: /NoteRequest/Details/5
        public IActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var request = _db.NoteRequests
                .Include(r => r.Major)
                .Include(r => r.Term)
                .Include(r => r.User)
                .Include(r => r.Messages).ThenInclude(m => m.Sender)
                .FirstOrDefault(r => r.Id == id && r.UserId == CurrentUserId());

            if (request == null) return NotFound();

            return View(request);
        }

        // POST: /NoteRequest/SendMessage
        [HttpPost]
        public IActionResult SendMessage(int requestId, string message)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var request = _db.NoteRequests.FirstOrDefault(r => r.Id == requestId && r.UserId == CurrentUserId());
            if (request == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(message))
            {
                _db.RequestMessages.Add(new RequestMessage
                {
                    NoteRequestId = requestId,
                    SenderId = CurrentUserId(),
                    Message = message.Trim(),
                    IsAdmin = false,
                    SentAt = DateTime.Now
                });
                _db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = requestId });
        }
    }
}