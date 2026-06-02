using HelpyDog.Web.Data;
using HelpyDog.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            
            // TWORZENIE PIESKA JEŚLI NIE MA
            if (pet == null)
            {
                Random rnd = new Random();
                pet = new Pet { Name = "Twój Piesek", UserId = userId, DogType = rnd.Next(1, 6) };
                _context.Pets.Add(pet);
                _context.SaveChanges();
            }

            // MECHANIZM SPADKU SZCZĘŚCIA (Grywalizacja)
            // Szukamy, kiedy użytkownik ostatnio się uczył
            var lastLog = _context.ActivityLogs
                                  .Where(a => a.PetId == pet.Id)
                                  .OrderByDescending(a => a.DateLogged)
                                  .FirstOrDefault();

            if (lastLog != null)
            {
                // Obliczamy ile dni minęło od ostatniej nauki
                var daysSinceLastStudy = (DateTime.UtcNow - lastLog.DateLogged).TotalDays;

                // Jeśli minęły więcej niż 2 dni, piesek traci 10 szczęścia za każdy dzień lenistwa
                if (daysSinceLastStudy >= 2)
                {
                    int happinessPenalty = (int)(daysSinceLastStudy * 10);
                    pet.HappinessLevel -= happinessPenalty;
                    
                    if (pet.HappinessLevel < 0) pet.HappinessLevel = 0; // Szczęście nie może spaść poniżej 0
                    
                    _context.SaveChanges();
                }
            }

            ViewBag.Habits = _context.Habits.ToList(); 
            return View(pet);
        }

        // (Tutaj zostaw swoją metodę LogSession bez zmian)
        // (Tutaj zostaw swoją metodę Leaderboard bez zmian)

        // NOWE: STATYSTYKI TYGODNIOWE (Wymóg laboratorium)
        public IActionResult WeeklyStats()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            // Zbieramy logi z ostatnich 7 dni, dołączamy informacje o Nawyku z tabeli Habits (żeby znać jego nazwę)
            var weeklyLogs = _context.ActivityLogs
                .Include(a => a.Habit)
                .Where(a => a.PetId == pet.Id && a.DateLogged >= sevenDaysAgo)
                .OrderByDescending(a => a.DateLogged)
                .ToList();

            return View(weeklyLogs);
        }
    }
}