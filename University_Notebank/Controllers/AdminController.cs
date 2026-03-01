using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using University_Notebank.Models;

namespace University_Notebank.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private bool IsAdmin() =>
            HttpContext.Session.GetString("UserRole") == "Admin";

        private List<SelectListItem> GetBatches() =>
            Enumerable.Range(10, 21)
                .Select(b => new SelectListItem
                {
                    Value = b.ToString(),
                    Text = "Batch " + b
                }).ToList();

        // GET: /Admin
        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var notes = _db.Notes
                .Include(n => n.Major)
                .Include(n => n.Term)
                .Include(n => n.Uploader)
                .ToList();

            // ✅ Pending notes আলাদা করে পাঠাও
            ViewBag.PendingNotes = notes.Where(n => n.Status == "Pending").ToList();

            return View(notes.Where(n => n.Status == "Approved").ToList());
        }

        // ✅ POST: /Admin/Approve/5
        [HttpPost]
        public IActionResult Approve(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var note = _db.Notes.Find(id);
            if (note == null) return NotFound();

            note.Status = "Approved";
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ POST: /Admin/Reject/5
        [HttpPost]
        public IActionResult Reject(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var note = _db.Notes.Find(id);
            if (note == null) return NotFound();

            note.Status = "Rejected";
            _db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /Admin/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

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

                Batches = GetBatches()
            };

            return View(model);
        }

        // POST: /Admin/Create
        [HttpPost]
        public async Task<IActionResult> Create(NoteFormViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

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
                Status = "Approved" // ✅ Admin নিজে upload করলে সরাসরি Approved
            };

            _db.Notes.Add(note);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Admin/Edit/5
        public IActionResult Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var note = _db.Notes.Find(id);
            if (note == null) return NotFound();

            var model = new NoteFormViewModel
            {
                Id = note.Id,
                Title = note.Title,
                Description = note.Description,
                MajorId = note.MajorId,
                TermId = note.TermId,
                Batch = note.Batch,

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

                Batches = GetBatches()
            };

            return View(model);
        }

        // POST: /Admin/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(NoteFormViewModel model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var note = _db.Notes.Find(model.Id);
            if (note == null) return NotFound();

            note.Title = model.Title;
            note.Description = model.Description;
            note.MajorId = model.MajorId;
            note.TermId = model.TermId;
            note.Batch = model.Batch;

            if (model.CoverImage != null)
            {
                var imgName = Guid.NewGuid() + Path.GetExtension(model.CoverImage.FileName);
                var savePath = Path.Combine(_env.WebRootPath, "images", "covers", imgName);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                using var stream = new FileStream(savePath, FileMode.Create);
                await model.CoverImage.CopyToAsync(stream);
                note.CoverImagePath = "/images/covers/" + imgName;
            }

            if (model.NoteFile != null)
            {
                note.FileName = model.NoteFile.FileName;
                note.FileType = Path.GetExtension(model.NoteFile.FileName).ToLower();
                note.FileSize = model.NoteFile.Length;

                var safeFileName = Guid.NewGuid() + note.FileType;
                var fileSave = Path.Combine(_env.WebRootPath, "uploads", "notes", safeFileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fileSave));
                using var fileStream = new FileStream(fileSave, FileMode.Create);
                await model.NoteFile.CopyToAsync(fileStream);
                note.FilePath = "/uploads/notes/" + safeFileName;
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Admin/Delete/5
        public IActionResult Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var note = _db.Notes
                .Include(n => n.Major)
                .Include(n => n.Term)
                .FirstOrDefault(n => n.Id == id);

            if (note == null) return NotFound();
            return View(note);
        }

        // POST: /Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var note = _db.Notes.Find(id);

            if (note != null)
            {
                if (!string.IsNullOrEmpty(note.FilePath))
                {
                    var fullFilePath = Path.Combine(_env.WebRootPath,
                        note.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fullFilePath))
                        System.IO.File.Delete(fullFilePath);
                }

                if (!string.IsNullOrEmpty(note.CoverImagePath))
                {
                    var fullImgPath = Path.Combine(_env.WebRootPath,
                        note.CoverImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fullImgPath))
                        System.IO.File.Delete(fullImgPath);
                }

                _db.Notes.Remove(note);
                _db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}