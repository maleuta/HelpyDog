using HelpyDog.Web.Data;
using HelpyDog.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelpyDog.Web.Controllers.Api
{
    [Route("api/activities")]
    [ApiController]
    public class ActivityApiController : ControllerBase
    {
        private readonly HelpyDogContext _context;

        public ActivityApiController(HelpyDogContext context)
        {
            _context = context;
        }

        public class ActivityPayload
        {
            public int HabitId { get; set; }
            public int DurationMinutes { get; set; }
        }

        [HttpPost]
        public IActionResult LogActivity([FromHeader] string username, [FromHeader] string token, [FromBody] ActivityPayload payload)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.Id == token);
            if (user == null) return Unauthorized("Błąd autoryzacji.");

            var pet = _context.Pets.FirstOrDefault(p => p.UserId == user.Id);
            var habit = _context.Habits.FirstOrDefault(h => h.Id == payload.HabitId);

            if (pet == null || habit == null) return BadRequest("Błąd danych.");

            int earnedXp = (int)(payload.DurationMinutes * habit.XpMultiplier);
            pet.ExperiencePoints += earnedXp;
            
            if (pet.ExperiencePoints >= pet.Level * 100)
            {
                pet.Level++;
                pet.ExperiencePoints = 0;
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                DurationMinutes = payload.DurationMinutes, EarnedXp = earnedXp, HabitId = payload.HabitId, PetId = pet.Id
            });
            _context.SaveChanges();

            return Ok(new { Message = "Zapisano!", Level = pet.Level });
        }
    }
}