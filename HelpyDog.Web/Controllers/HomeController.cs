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

            if (pet == null)
            {
                Random rnd = new Random();
                pet = new Pet { Name = "Twój Piesek", UserId = userId, DogType = rnd.Next(1, 6) };
                _context.Pets.Add(pet);
                _context.SaveChanges();
            }

            return View(pet);
        }

        // Odbiera czas z formularza na stronie
        [HttpPost]
        public IActionResult LogSession(int durationMinutes)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            
            // Pobieramy domyślny nawyk (np. Nauka programowania)
            var habit = _context.Habits.FirstOrDefault(); 

            if (pet != null && habit != null && durationMinutes > 0)
            {
                int earnedXp = (int)(durationMinutes * habit.XpMultiplier);
                pet.ExperiencePoints += earnedXp;
                
                if (pet.ExperiencePoints >= pet.Level * 100)
                {
                    pet.Level++;
                    pet.ExperiencePoints = 0; // Zerujemy punkty po awansie
                }

                _context.ActivityLogs.Add(new ActivityLog
                {
                    DurationMinutes = durationMinutes,
                    EarnedXp = earnedXp,
                    HabitId = habit.Id,
                    PetId = pet.Id
                });
                
                _context.SaveChanges();
            }

            return RedirectToAction("Index"); // Przeładowuje stronę główną
        }

        //Widok innych uczestników (Ranking)
        public IActionResult Leaderboard()
        {
            // Pobieramy wszystkie zwierzaki i ich właścicieli, sortujemy po poziomie
            var allPets = _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.ExperiencePoints)
                .ToList();

            return View(allPets);
        }
    }
}