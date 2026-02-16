using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using System.Security.Claims;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;

        public AuthController(
            AppDbContext context,
            PasswordService passwordService,
            JwtService jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        // ================= REGISTER =================
        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Email == request.Email))
                return BadRequest("User already exists");

            // ✅ 1) ننشئ User Entity من بيانات الـ DTO
            var user = new User
            {
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                Role = "User"
            };

            // ✅ 2) نعمل hash بعد ما صار عندنا user object
            user.PasswordHash = _passwordService.HashPassword(user, request.Password);

            // ✅ 3) نحفظ بالـ DB
            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }


        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            // ✅ صار VerifyPassword بدو user object
            var isPasswordValid = _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password
            );

            if (!isPasswordValid)
                return Unauthorized("Invalid email or password");

            // ⚠️ إذا عدّلتي JwtService سابقاً ليشمل userId:
            // var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
            // إذا لسه القديم:
            // var token = _jwtService.GenerateToken(user.Email, user.Role);

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role);

            return Ok(new { Token = token });
        }

        // ================= PROTECTED ENDPOINT =================
        [Authorize]
        [HttpGet("secret")]
        public IActionResult Secret()
        {
            return Ok("You are authorized!");
        }
        // ================= CURRENT USER (ME) =================
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            // ✅ read claims from token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            // ✅ safety check
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Missing userId claim in token." });

            return Ok(new
            {
                userId,
                email,
                role
            });
        }


        // ✅ Admin-only endpoint
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-secret")]
        public IActionResult AdminSecret()
        {
            return Ok("You are ADMIN!");
        }

    }
}