using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using University_Notebank.Models;

namespace University_Notebank.Controllers
{
    public class NotesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public NotesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Notes
        public IActionResult Index(int? majorId, int? termId, int? batch)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Account");

            var notes = _db.Notes
                .Include(n => n.Major)
                .Include(n => n.Term)
                .Include(n => n.Uploader)
                .Where(n => n.Status == "Approved") // ✅ শুধু Approved notes
                .AsQueryable();

            if (majorId.HasValue)
                notes = notes.Where(n => n.MajorId == majorId);
            if (termId.HasValue)
                notes = notes.Where(n => n.TermId == termId);
            if (batch.HasValue)
                notes = notes.Where(n => n.Batch == batch);

            ViewBag.Majors = _db.Majors.ToList();
            ViewBag.Terms = _db.Terms.ToList();
            ViewBag.SelectedMajor = majorId;
            ViewBag.SelectedTerm = termId;
            ViewBag.SelectedBatch = batch;

            return View(notes.ToList());
        }

        // GET: /Notes/Details/5
        public IActionResult Details(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Account");

            var note = _db.Notes
                .Include(n => n.Major)
                .Include(n => n.Term)
                .Include(n => n.Uploader)
                .FirstOrDefault(n => n.Id == id);

            if (note == null) return NotFound();
            return View(note);
        }

        // GET: /Notes/Upload
        public IActionResult Upload()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Account");

            var model = new NoteFormViewModel
            {
                Majors = _db.Majors
                    .Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = m.Name
                    }).ToList(),

                Terms = _db.Terms
                    .Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = t.Name
                    }).ToList(),

                Batches = Enumerable.Range(10, 21)
                    .Select(b => new SelectListItem
                    {
                        Value = b.ToString(),
                        Text = "Batch " + b
                    }).ToList()
            };

            return View(model);
        }

        // POST: /Notes/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(NoteFormViewModel model)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Account");

            string imagePath = null;
            string filePath = null;
            string fileName = null;
            string fileType = null;
            long fileSize = 0;

            if (model.CoverImage != null)
            {
                var imgName = Guid.NewGuid() + Path.GetExtension(model.CoverImage.FileName);
                var imgSave = Path.Combine(_env.WebRootPath, "images", "covers", imgName);
                Directory.CreateDirectory(Path.GetDirectoryName(imgSave));
                using var imgStream = new FileStream(imgSave, FileMode.Create);
                await model.CoverImage.CopyToAsync(imgStream);
                imagePath = "/images/covers/" + imgName;
            }

            if (model.NoteFile != null)
            {
                fileName = model.NoteFile.FileName;
                fileType = Path.GetExtension(model.NoteFile.FileName).ToLower();
                fileSize = model.NoteFile.Length;

                var safeFileName = Guid.NewGuid() + fileType;
                var fileSave = Path.Combine(_env.WebRootPath, "uploads", "notes", safeFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fileSave));
                using var fileStream = new FileStream(fileSave, FileMode.Create);
                await model.NoteFile.CopyToAsync(fileStream);
                filePath = "/uploads/notes/" + safeFileName;
            }

            var note = new Note
            {
                Title = model.Title,
                Description = model.Description,
                MajorId = model.MajorId,
                TermId = model.TermId,
                Batch = model.Batch,
                Level = null,
                CoverImagePath = imagePath,
                FilePath = filePath,
                FileName = fileName,
                FileType = fileType,
                FileSize = fileSize,
                UploaderId = (int)HttpContext.Session.GetInt32("UserId"),
                Status = "Pending" // ✅ Admin approve করার আগে Pending
            };

            _db.Notes.Add(note);
            _db.SaveChanges();

            TempData["UploadSuccess"] = "Your note has been submitted and is awaiting admin approval.";
            return RedirectToAction("MyUploads");
        }

        // GET: /Notes/MyUploads
        public IActionResult MyUploads()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Account");

            int userId = (int)HttpContext.Session.GetInt32("UserId");

            var notes = _db.Notes
                .Include(n => n.Major)
                .Include(n => n.Term)
                .Where(n => n.UploaderId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notes);
        }
    }
}