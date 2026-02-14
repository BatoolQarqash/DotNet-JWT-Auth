using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

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
            {
                return BadRequest("User already exists");
            }

            var hashedPassword = _passwordService.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok("User registered successfully");
        }

        // ================= LOGIN =================
        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var isPasswordValid = _passwordService.VerifyPassword(
                user.PasswordHash,
                request.Password
            );

            if (!isPasswordValid)
            {
                return Unauthorized("Invalid email or password");
            }

            var token = _jwtService.GenerateToken(user.Email);

            return Ok(new
            {
                Token = token
            });
        }

        // ================= PROTECTED ENDPOINT =================
        [Authorize]
        [HttpGet("secret")]
        public IActionResult Secret()
        {
            return Ok("You are authorized!");
        }
    }
}
