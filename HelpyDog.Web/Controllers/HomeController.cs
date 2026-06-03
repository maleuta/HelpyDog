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
            
            // Zamiast losować, odsyłamy do wyboru
            if (pet == null)
            {
                return RedirectToAction("CreatePet");
            }

            // Mechanizm spadku szczęścia
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

        // EKRAN ADOPCJI (WYBÓR PIESKA)
        [HttpGet]
        public IActionResult CreatePet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            // Jeśli już ma pieska, nie pozwalamy mu wejść na tę stronę
            if (_context.Pets.Any(p => p.UserId == userId)) return RedirectToAction("Index");

            return View();
        }

        [HttpPost]
        public IActionResult CreatePet(string petName, int dogType)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            if (!_context.Pets.Any(p => p.UserId == userId))
            {
                var newPet = new Pet
                {
                    Name = string.IsNullOrWhiteSpace(petName) ? "Mój Pomocnik" : petName,
                    UserId = userId,
                    DogType = dogType
                };
                _context.Pets.Add(newPet);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        
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

        public IActionResult Leaderboard()
        {
            var allPets = _context.Pets
                .Include(p => p.User)
                .OrderByDescending(p => p.Level)
                .ThenByDescending(p => p.ExperiencePoints)
                .ToList();

            return View(allPets);
        }

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
                    pet.ExperiencePoints -= item.PriceXp;
                    pet.HappinessLevel += item.HappinessRestoreValue;
                    if (pet.HappinessLevel > 100) pet.HappinessLevel = 100; 

                    string imageFile = "flower.jpg";
                    string nameLower = item.Name.ToLower();
                    
                    if (nameLower.Contains("rybk")) imageFile = "fish.png";
                    else if (nameLower.Contains("kot")) imageFile = "cat.png";

                    pet.OwnedItems += imageFile + ";"; 

                    _context.SaveChanges();
                    TempData["Message"] = $"Kupiłeś: {item.Name}! Zobaczysz to teraz obok pieska.";
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