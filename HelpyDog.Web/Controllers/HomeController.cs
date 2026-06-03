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

            var lastLog = _context.ActivityLogs
                                  .Where(a => a.PetId == pet.Id)
                                  .OrderByDescending(a => a.DateLogged)
                                  .FirstOrDefault();

            if (lastLog != null)
            {
                var daysSinceLastStudy = (DateTime.UtcNow - lastLog.DateLogged).TotalDays;
                if (daysSinceLastStudy >= 2)
                {
                    int happinessPenalty = (int)(daysSinceLastStudy * 10);
                    pet.HappinessLevel -= happinessPenalty;
                    if (pet.HappinessLevel < 0) pet.HappinessLevel = 0; 
                    _context.SaveChanges();
                }
            }

            ViewBag.Habits = _context.Habits.ToList(); 
            return View(pet);
        }

        // PRZYWRÓCONA METODA: DODAWANIE CZASU
        [HttpPost]
        public IActionResult LogSession(int durationMinutes, int habitId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            var habit = _context.Habits.FirstOrDefault(h => h.Id == habitId); 

            if (pet != null && habit != null && durationMinutes > 0)
            {
                int earnedXp = (int)(durationMinutes * habit.XpMultiplier);
                pet.ExperiencePoints += earnedXp;
                
                if (pet.ExperiencePoints >= pet.Level * 100)
                {
                    pet.Level++;
                    pet.ExperiencePoints -= (pet.Level - 1) * 100;
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
            return RedirectToAction("Index");
        }

        // PRZYWRÓCONA METODA: RANKING (LEADERBOARD)
        public IActionResult Leaderboard()
        {
            var allPets = _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.ExperiencePoints)
                .ToList();

            return View(allPets);
        }

        // STATYSTYKI TYGODNIOWE
        public IActionResult WeeklyStats()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var weeklyLogs = _context.ActivityLogs
                .Include(a => a.Habit)
                .Where(a => a.PetId == pet.Id && a.DateLogged >= sevenDaysAgo)
                .OrderByDescending(a => a.DateLogged)
                .ToList();

            return View(weeklyLogs);
        }

        // ============================================
        // NOWOŚĆ: SKLEP ZA XP
        // ============================================
        public IActionResult Store()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            ViewBag.PetXp = pet?.ExperiencePoints ?? 0;

            var items = _context.ShopItems.ToList();
            return View(items);
        }

        [HttpPost]
        public IActionResult BuyItem(int itemId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var pet = _context.Pets.FirstOrDefault(p => p.UserId == userId);
            var item = _context.ShopItems.FirstOrDefault(i => i.Id == itemId);

            if (pet != null && item != null)
            {
                if (pet.ExperiencePoints >= item.PriceXp)
                {
                    // Płacimy w XP, zyskujemy szczęście!
                    pet.ExperiencePoints -= item.PriceXp;
                    pet.HappinessLevel += item.HappinessRestoreValue;
                    if (pet.HappinessLevel > 100) pet.HappinessLevel = 100; // Blokada na max 100%

                    _context.SaveChanges();
                    TempData["Message"] = $"Kupiłeś: {item.Name}! Piesek jest szczęśliwszy.";
                }
                else
                {
                    TempData["Error"] = "Masz za mało punktów XP!";
                }
            }
            return RedirectToAction("Store");
        }
    }
}