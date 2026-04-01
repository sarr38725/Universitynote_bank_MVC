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

        // GET: /RelationQuestion
        public IActionResult Index(int? majorId, int? termId, string? status)
        {
            var query = _db.RelationQuestions
                .Include(q => q.User)
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Note)
                .Include(q => q.Answers)
                .AsQueryable();

            if (majorId.HasValue)
                query = query.Where(q => q.MajorId == majorId);

            if (termId.HasValue)
                query = query.Where(q => q.TermId == termId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(q => q.Status == status);

            var questions = query.OrderByDescending(q => q.CreatedAt).ToList();

            ViewBag.Majors = new SelectList(_db.Majors.ToList(), "Id", "Name", majorId);
            ViewBag.Terms = new SelectList(_db.Terms.ToList(), "Id", "Name", termId);
            ViewBag.SelectedStatus = status;

            return View(questions);
        }

        // GET: /RelationQuestion/MyQuestions
        public IActionResult MyQuestions()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var questions = _db.RelationQuestions
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Answers)
                .Where(q => q.UserId == CurrentUserId())
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            return View(questions);
        }

        // GET: /RelationQuestion/Create
        public IActionResult Create()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            ViewBag.Majors = new SelectList(_db.Majors.ToList(), "Id", "Name");
            ViewBag.Terms = new SelectList(_db.Terms.ToList(), "Id", "Name");
            ViewBag.Notes = new SelectList(
                _db.Notes.Where(n => n.Status == "Approved").Select(n => new { n.Id, n.Title }).ToList(),
                "Id", "Title");

            return View();
        }

        // POST: /RelationQuestion/Create
        [HttpPost]
        public IActionResult Create(RelationQuestion model)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            model.UserId = CurrentUserId();
            model.Status = "Open";
            model.CreatedAt = DateTime.Now;

            _db.RelationQuestions.Add(model);
            _db.SaveChanges();

            TempData["Success"] = "আপনার প্রশ্নটি সফলভাবে পোস্ট করা হয়েছে!";
            return RedirectToAction("MyQuestions");
        }

        // GET: /RelationQuestion/Details/5
        public IActionResult Details(int id)
        {
            var question = _db.RelationQuestions
                .Include(q => q.User)
                .Include(q => q.Major)
                .Include(q => q.Term)
                .Include(q => q.Note)
                .Include(q => q.Answers).ThenInclude(a => a.User)
                .FirstOrDefault(q => q.Id == id);

            if (question == null) return NotFound();

            return View(question);
        }

        // POST: /RelationQuestion/PostAnswer
        [HttpPost]
        public IActionResult PostAnswer(int questionId, string body)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var question = _db.RelationQuestions.FirstOrDefault(q => q.Id == questionId);
            if (question == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(body))
            {
                _db.QuestionAnswers.Add(new QuestionAnswer
                {
                    QuestionId = questionId,
                    UserId = CurrentUserId(),
                    Body = body.Trim(),
                    IsAccepted = false,
                    CreatedAt = DateTime.Now
                });

                if (question.Status == "Open")
                {
                    question.Status = "Answered";
                }

                _db.SaveChanges();
            }

            return RedirectToAction("Details", new { id = questionId });
        }

        // POST: /RelationQuestion/AcceptAnswer
        [HttpPost]
        public IActionResult AcceptAnswer(int answerId)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var answer = _db.QuestionAnswers
                .Include(a => a.Question)
                .FirstOrDefault(a => a.Id == answerId);

            if (answer == null) return NotFound();

            // Only the question owner can accept an answer
            if (answer.Question.UserId != CurrentUserId())
                return Forbid();

            // Unaccept any previously accepted answer for this question
            var existing = _db.QuestionAnswers
                .Where(a => a.QuestionId == answer.QuestionId && a.IsAccepted)
                .ToList();
            foreach (var a in existing)
                a.IsAccepted = false;

            answer.IsAccepted = true;
            answer.Question.Status = "Answered";
            _db.SaveChanges();

            return RedirectToAction("Details", new { id = answer.QuestionId });
        }

        // POST: /RelationQuestion/CloseQuestion
        [HttpPost]
        public IActionResult CloseQuestion(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");

            var question = _db.RelationQuestions.FirstOrDefault(q => q.Id == id && q.UserId == CurrentUserId());
            if (question == null) return NotFound();

            question.Status = "Closed";
            _db.SaveChanges();

            return RedirectToAction("Details", new { id });
        }
    }
}
