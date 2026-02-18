using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Data
{
    public class Post
    {
        public int Id { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        // مسار/رابط الصورة المخزن (مثال: "/uploads/abc.jpg")
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // ملاحظة اختيارية (Admin note / caption / source…)
        [MaxLength(500)]
        public string? Note { get; set; }

        // FK
        [Required]
        public int CategoryId { get; set; }

        // Navigation
        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
