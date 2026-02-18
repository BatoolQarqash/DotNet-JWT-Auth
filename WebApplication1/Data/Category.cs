using System.ComponentModel.DataAnnotations;

using WebApplication1.Models;

namespace WebApplication1.Data

{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        // Navigation (اختياري)
        public List<Post> Posts { get; set; } = new();
    }
}
