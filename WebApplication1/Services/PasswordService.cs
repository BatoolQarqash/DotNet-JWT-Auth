using Microsoft.AspNetCore.Identity;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<User> _hasher = new();

        // ✅ Hash password
        public string HashPassword(User user, string password)
        {
            // user here is used only as a context for hashing
            return _hasher.HashPassword(user, password);
        }

        // ✅ Verify password
        public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(
                user,
                hashedPassword,
                providedPassword
            );

            return result == PasswordVerificationResult.Success;
        }
    }
}
