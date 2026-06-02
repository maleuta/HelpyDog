using HelpyDog.Web.Data;
using HelpyDog.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelpyDog.Web.Controllers
{
    public class HabitsController : Controller
    {
        private readonly HelpyDogContext _context;

        public HabitsController(HelpyDogContext context)
        {
            _context = context;
        }

        // WYŚWIETLANIE LISTY (READ)
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var habits = _context.Habits.ToList();
            return View(habits);
        }

        // FORMULARZ DODAWANIA (CREATE - GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ZAPISYWANIE NOWEJ AKTYWNOŚCI (CREATE - POST)
        [HttpPost]
        public IActionResult Create(Habit habit)
        {
            // Domyślnie ustawiamy IsGlobal na true, żeby wszyscy widzieli tę aktywność
            habit.IsGlobal = true; 

            _context.Habits.Add(habit);
            _context.SaveChanges();
            
            return RedirectToAction("Index");
        }

        // USUWANIE (DELETE)
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var habit = _context.Habits.Find(id);
            if (habit != null)
            {
                _context.Habits.Remove(habit);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}