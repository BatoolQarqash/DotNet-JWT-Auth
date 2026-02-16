using System.Collections.Generic;
namespace WebApplication1.Data
{
    public class User
    {
        public int Id { get; set; }
         public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public  DateTime CreatedAt { get; set; }
       
        // ✅ NEW: Role (Admin/User)
        public string Role { get; set; } = "User";
        public List<Note> Notes { get; set; } = new();

    }
}
