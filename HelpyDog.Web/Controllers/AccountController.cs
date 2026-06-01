using HelpyDog.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace HelpyDog.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly HelpyDogContext _context;

        public AccountController(HelpyDogContext context)
        {
            _context = context;
        }

        // Wyświetla formularz logowania
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Przetwarza dane z formularza po kliknięciu "Zaloguj"
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var hashedPw = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.PasswordHash == hashedPw);

            if (user != null)
            {
                // Zapisujemy dane do sesji (wymóg z laboratorium!)
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("UserRole", user.Role);
                
                // Po udanym logowaniu idziemy do pieska
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Nieprawidłowy login lub hasło.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password ?? ""));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}