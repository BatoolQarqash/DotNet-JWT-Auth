using System;
namespace WebApplication1.Data
{
    public class Note
    {
        public int Id { get; set; }

        // محتوى الملاحظة
        public string Title { get; set; }
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Foreign Key: يربط الملاحظة بالمستخدم
        public int UserId { get; set; }

        // ✅ Navigation Property: تساعد EF Core يفهم العلاقة
        public User User { get; set; }
    }
}
