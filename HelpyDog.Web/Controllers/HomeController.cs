using HelpyDog.Web.Data;
using HelpyDog.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelpyDog.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly HelpyDogContext _context;

        public HomeController(HelpyDogContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account"); // Upewnij się, że masz AccountController!
            }

            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);

            if (pet == null)
            {
                Random rnd = new Random();
                pet = new Pet
                {
                    Name = "Twój Piesek",
                    UserId = userId,
                    DogType = rnd.Next(1, 6)
                };
                
                _context.Pets.Add(pet);
                _context.SaveChanges();
            }

            return View(pet);
        }
    }
}