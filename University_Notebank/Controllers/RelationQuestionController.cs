using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using University_Notebank.Models;

namespace University_Notebank.Controllers
{
    public class RelationQuestionController : Controller
    {
        private readonly AppDbContext _db;

        public RelationQuestionController(AppDbContext db)
        {
            _db = db;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetString("UserId") != null;
        private int CurrentUserId() => (int)HttpContext.Session.GetInt32("UserId");
        private string CurrentRole() => HttpContext.Session.GetString("UserRole") ?? "";
        private bool IsTeacher() => CurrentRole() == "Teacher" || CurrentRole() == "Admin";

        // ─────────────────────────────────────────────
        // STUDENT: Browse all published teacher questions
        // GET: /RelationQuestion
        // ─────────────────────────────────────────────
        public IActionResult Index(int? majorId, int? termId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var query = _db.RelationQuestions
                .Include(q => q.User)
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Note)
                .Include(q => q.Answers)
                .Where(q => q.IsPublished && q.Status != "Closed")
                .AsQueryable();

            if (majorId.HasValue)
                query = query.Where(q => q.MajorId == majorId);

            if (termId.HasValue)
                query = query.Where(q => q.TermId == termId);

            var questions = query.OrderByDescending(q => q.CreatedAt).ToList();

            ViewBag.Majors = new SelectList(_db.Majors.ToList(), "Id", "Name", majorId);
            ViewBag.Terms = new SelectList(_db.Terms.ToList(), "Id", "Name", termId);

            return View(questions);
        }

        // ─────────────────────────────────────────────
        // TEACHER: Dashboard — own questions list
        // GET: /RelationQuestion/TeacherPanel
        // ─────────────────────────────────────────────
        public IActionResult TeacherPanel()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            var questions = _db.RelationQuestions
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Answers)
                .Where(q => q.UserId == CurrentUserId())
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            return View(questions);
        }

        // ─────────────────────────────────────────────
        // TEACHER: Create question form
        // GET: /RelationQuestion/Create
        // ─────────────────────────────────────────────
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            ViewBag.Majors = new SelectList(_db.Majors.ToList(), "Id", "Name");
            ViewBag.Terms = new SelectList(_db.Terms.ToList(), "Id", "Name");
            ViewBag.Notes = new SelectList(
                _db.Notes.Where(n => n.Status == "Approved").Select(n => new { n.Id, n.Title }).ToList(),
                "Id", "Title");

            return View();
        }

        // POST: /RelationQuestion/Create
        [HttpPost]
        public IActionResult Create(RelationQuestion model, string? publish)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            model.UserId = CurrentUserId();
            model.CreatedAt = DateTime.Now;
            model.Status = "Open";
            model.IsPublished = (publish == "1");

            _db.RelationQuestions.Add(model);
            _db.SaveChanges();

            TempData["Success"] = model.IsPublished
                ? "প্রশ্নটি publish করা হয়েছে — students এখন দেখতে পাবে।"
                : "প্রশ্নটি draft হিসেবে সংরক্ষিত হয়েছে।";

            return RedirectToAction("TeacherPanel");
        }

        // ─────────────────────────────────────────────
        // TEACHER: Publish a draft question
        // POST: /RelationQuestion/Publish/5
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult Publish(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            var q = _db.RelationQuestions.FirstOrDefault(x => x.Id == id && x.UserId == CurrentUserId());
            if (q == null) return NotFound();

            q.IsPublished = true;
            _db.SaveChanges();

            TempData["Success"] = "প্রশ্নটি students-দের কাছে publish করা হয়েছে।";
            return RedirectToAction("TeacherPanel");
        }

        // ─────────────────────────────────────────────
        // TEACHER: Close a question (stop accepting answers)
        // POST: /RelationQuestion/CloseQuestion/5
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult CloseQuestion(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            var q = _db.RelationQuestions.FirstOrDefault(x => x.Id == id && x.UserId == CurrentUserId());
            if (q == null) return NotFound();

            q.Status = "Closed";
            _db.SaveChanges();

            return RedirectToAction("TeacherPanel");
        }

        // ─────────────────────────────────────────────
        // SHARED: View question details
        // GET: /RelationQuestion/Details/5
        // ─────────────────────────────────────────────
        public IActionResult Details(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var question = _db.RelationQuestions
                .Include(q => q.User)
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Note)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefault(q => q.Id == id);

            if (question == null) return NotFound();

            // Students can only see published questions
            if (!IsTeacher() && !question.IsPublished)
                return NotFound();

            // Check if the current student already answered
            int uid = CurrentUserId();
            ViewBag.AlreadyAnswered = question.Answers.Any(a => a.UserId == uid);

            return View(question);
        }

        // ─────────────────────────────────────────────
        // STUDENT: Submit answer
        // POST: /RelationQuestion/PostAnswer
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult PostAnswer(int questionId, string body)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var question = _db.RelationQuestions.FirstOrDefault(q => q.Id == questionId);
            if (question == null || !question.IsPublished || question.Status == "Closed")
                return NotFound();

            int uid = CurrentUserId();
            bool alreadyAnswered = _db.QuestionAnswers.Any(a => a.QuestionId == questionId && a.UserId == uid);
            if (alreadyAnswered)
            {
                TempData["Error"] = "আপনি এই প্রশ্নের উত্তর ইতিমধ্যে দিয়েছেন।";
                return RedirectToAction("Details", new { id = questionId });
            }

            if (!string.IsNullOrWhiteSpace(body))
            {
                _db.QuestionAnswers.Add(new QuestionAnswer
                {
                    QuestionId = questionId,
                    UserId = uid,
                    Body = body.Trim(),
                    CreatedAt = DateTime.Now
                });

                if (question.Status == "Open")
                    question.Status = "Active";

                _db.SaveChanges();
                TempData["Success"] = "আপনার উত্তর জমা দেওয়া হয়েছে!";
            }

            return RedirectToAction("Details", new { id = questionId });
        }

        // ─────────────────────────────────────────────
        // TEACHER: View all student answers for a question
        // GET: /RelationQuestion/ViewAnswers/5
        // ─────────────────────────────────────────────
        public IActionResult ViewAnswers(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            var question = _db.RelationQuestions
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefault(q => q.Id == id && q.UserId == CurrentUserId());

            if (question == null) return NotFound();

            return View(question);
        }

        // ─────────────────────────────────────────────
        // TEACHER: Give marks & feedback on a student answer
        // POST: /RelationQuestion/MarkAnswer
        // ─────────────────────────────────────────────
        [HttpPost]
        public IActionResult MarkAnswer(int answerId, int? marks, string? feedback, bool accept = false)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            if (!IsTeacher()) return RedirectToAction("Index");

            var answer = _db.QuestionAnswers
                .Include(a => a.Question)
                .FirstOrDefault(a => a.Id == answerId && a.Question.UserId == CurrentUserId());

            if (answer == null) return NotFound();

            answer.Marks = marks;
            answer.TeacherFeedback = feedback?.Trim();
            answer.IsAccepted = accept;

            _db.SaveChanges();

            return RedirectToAction("ViewAnswers", new { id = answer.QuestionId });
        }
    }
}
