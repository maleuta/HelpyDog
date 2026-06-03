using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HelpyDog.Web.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        public virtual Pet Pet { get; set; }
    }

    public class Pet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; } = 0;
        public int HappinessLevel { get; set; } = 100;
        
        public int DogType { get; set; } = 1; 

        // NOWOŚĆ: Ta linijka będzie przechowywać Twoje kupione przedmioty!
        public string OwnedItems { get; set; } = "";

        [Required]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }

    public class Habit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public double XpMultiplier { get; set; } = 1.0;
        public bool IsGlobal { get; set; }

        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }

    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime DateLogged { get; set; } = DateTime.UtcNow;
        public int DurationMinutes { get; set; }
        public int EarnedXp { get; set; }

        public int HabitId { get; set; }
        [ForeignKey("HabitId")]
        public virtual Habit Habit { get; set; }

        public int PetId { get; set; }
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; }
    }

    public class ShopItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        public int PriceXp { get; set; }
        public int HappinessRestoreValue { get; set; }
    }
    
}

