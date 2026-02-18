using Microsoft.EntityFrameworkCore;
using WebApplication1.Services;

namespace WebApplication1.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

            const string adminEmail = "admin@blog.local";
            const string adminPassword = "Admin@12345";

            // إذا موجود مسبقاً، لا تعملي شيء
            var adminExists = await db.Users.AnyAsync(u => u.Email == adminEmail);
            if (adminExists) return;

            var admin = new User
            {
                Email = adminEmail,
                CreatedAt = DateTime.UtcNow,
                Role = "Admin"
            };

            admin.PasswordHash = passwordService.HashPassword(admin, adminPassword);

            db.Users.Add(admin);
            await db.SaveChangesAsync();

            Console.WriteLine($"✅ Admin seeded: {adminEmail} / {adminPassword}");
        }
    }
}
