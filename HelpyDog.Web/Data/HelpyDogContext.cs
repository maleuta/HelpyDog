using Microsoft.EntityFrameworkCore;
using HelpyDog.Web.Models;
using System.Security.Cryptography;
using System.Text;

namespace HelpyDog.Web.Data
{
    public class HelpyDogContext : DbContext
    {
        public HelpyDogContext(DbContextOptions<HelpyDogContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<ShopItem> ShopItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Pet)
                .WithOne(p => p.User)
                .HasForeignKey<Pet>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            string adminId = Guid.NewGuid().ToString();
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = adminId,
                UserName = "admin",
                PasswordHash = HashPassword("admin123"), 
                Role = "Admin"
            });

            modelBuilder.Entity<Habit>().HasData(
                new Habit { Id = 1, Title = "Nauka programowania", XpMultiplier = 1.5, IsGlobal = true }
            );
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
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