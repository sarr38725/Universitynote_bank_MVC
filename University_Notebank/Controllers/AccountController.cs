using Microsoft.AspNetCore.Mvc;
using University_Notebank.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace University_Notebank.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Register
        public IActionResult Register() => View();

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = model.Role
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login() => View();

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var hashed = HashPassword(model.Password);
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashed);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        public IActionResult Profile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login");

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var user = _db.Users.Find(userId);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        public IActionResult Profile(ProfileViewModel model)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login");

            int userId = (int)HttpContext.Session.GetInt32("UserId");
            var user = _db.Users.Find(userId);
            if (user == null) return NotFound();

            if (_db.Users.Any(u => u.Email == model.Email && u.Id != userId))
            {
                ModelState.AddModelError("", "Email already in use by another account.");
                model.Role = user.Role;
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) ||
                    user.PasswordHash != HashPassword(model.CurrentPassword))
                {
                    ModelState.AddModelError("", "Current password is incorrect.");
                    model.Role = user.Role;
                    return View(model);
                }
                user.PasswordHash = HashPassword(model.NewPassword);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;

            _db.SaveChanges();
            HttpContext.Session.SetString("UserName", user.FullName);

            TempData["ProfileSuccess"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}