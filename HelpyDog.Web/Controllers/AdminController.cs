using HelpyDog.Web.Data;
using HelpyDog.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HelpyDog.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly HelpyDogContext _context;

        public AdminController(HelpyDogContext context)
        {
            _context = context;
        }

        // Zabezpieczenie - sprawdzenie czy użytkownik to Admin
        private bool IsAdmin() => HttpContext.Session.GetString("UserRole") == "Admin";

        public IActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult Create(string username, string password, string role)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            if (_context.Users.Any(u => u.UserName == username))
            {
                ViewBag.Error = "Taki użytkownik już istnieje!";
                return View();
            }

            var newUser = new User
            {
                UserName = username,
                PasswordHash = HashPassword(password),
                Role = role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (var b in bytes) { builder.Append(b.ToString("x2")); }
                return builder.ToString();
            }
        }
    }
}