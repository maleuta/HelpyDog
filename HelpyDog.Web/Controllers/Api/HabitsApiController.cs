using HelpyDog.Web.Data;
using HelpyDog.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelpyDog.Web.Controllers.Api
{
    [Route("api/habits")]
    [ApiController]
    public class HabitsApiController : ControllerBase
    {
        private readonly HelpyDogContext _context;

        public HabitsApiController(HelpyDogContext context)
        {
            _context = context;
        }

        // Prywatna metoda do sprawdzania autoryzacji z nagłówków
        private bool IsAuthorized()
        {
            var username = Request.Headers["username"].ToString();
            var token = Request.Headers["token"].ToString();
            return _context.Users.Any(u => u.UserName == username && u.Id == token);
        }

        // 1. WYŚWIETLANIE (GET)
        [HttpGet]
        public IActionResult GetHabits()
        {
            if (!IsAuthorized()) return Unauthorized("Błąd autoryzacji");
            return Ok(_context.Habits.ToList());
        }

        // 2. DODAWANIE (POST)
        [HttpPost]
        public IActionResult CreateHabit([FromBody] Habit habit)
        {
            if (!IsAuthorized()) return Unauthorized("Błąd autoryzacji");
            
            _context.Habits.Add(habit);
            _context.SaveChanges();
            return Created($"/api/habits/{habit.Id}", habit);
        }

        // 3. MODYFIKACJA (PUT)
        [HttpPut("{id}")]
        public IActionResult UpdateHabit(int id, [FromBody] Habit updatedHabit)
        {
            if (!IsAuthorized()) return Unauthorized("Błąd autoryzacji");
            
            var habit = _context.Habits.Find(id);
            if (habit == null) return NotFound();

            habit.Title = updatedHabit.Title;
            habit.XpMultiplier = updatedHabit.XpMultiplier;
            
            _context.SaveChanges();
            return Ok(habit);
        }

        // 4. USUWANIE (DELETE)
        [HttpDelete("{id}")]
        public IActionResult DeleteHabit(int id)
        {
            if (!IsAuthorized()) return Unauthorized("Błąd autoryzacji");

            var habit = _context.Habits.Find(id);
            if (habit == null) return NotFound();

            _context.Habits.Remove(habit);
            _context.SaveChanges();
            return Ok(new { Message = "Nawyk usunięty" });
        }
    }
}