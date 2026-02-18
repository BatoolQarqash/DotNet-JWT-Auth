using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/admin/posts")]
    [Authorize(Roles = "Admin")]
    public class AdminPostsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminPostsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _db.Posts
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.ImageUrl,
                    p.Note,
                    p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var post = await _db.Posts
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Description,
                    p.ImageUrl,
                    p.Note,
                    p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    p.CreatedAt
                })
                .FirstOrDefaultAsync();

            return post == null ? NotFound() : Ok(post);
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10MB
        public async Task<IActionResult> Create([FromForm] UpsertPostForm form)
        {
            // تأكد category موجودة
            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == form.CategoryId);
            if (!categoryExists) return BadRequest("Invalid categoryId.");

            var post = new Post
            {
                Title = form.Title.Trim(),
                Description = form.Description.Trim(),
                Note = string.IsNullOrWhiteSpace(form.Note) ? null : form.Note.Trim(),
                CategoryId = form.CategoryId,
                CreatedAt = DateTime.UtcNow
            };

            if (form.Image != null)
            {
                post.ImageUrl = await SaveImageAsync(form.Image);
            }

            _db.Posts.Add(post);
            await _db.SaveChangesAsync();

            return Ok(new { post.Id });
        }

        [HttpPut("{id:int}")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] UpsertPostForm form)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();

            var categoryExists = await _db.Categories.AnyAsync(c => c.Id == form.CategoryId);
            if (!categoryExists) return BadRequest("Invalid categoryId.");

            post.Title = form.Title.Trim();
            post.Description = form.Description.Trim();
            post.Note = string.IsNullOrWhiteSpace(form.Note) ? null : form.Note.Trim();
            post.CategoryId = form.CategoryId;

            if (form.Image != null)
            {
                // Optional: delete old image file (ممكن نضيفها لاحقاً)
                post.ImageUrl = await SaveImageAsync(form.Image);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _db.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _db.Posts.Remove(post);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsPath, fileName);

            await using var stream = System.IO.File.Create(fullPath);
            await file.CopyToAsync(stream);

            // يرجع URL path
            return $"/uploads/{fileName}";
        }
    }

    public class UpsertPostForm
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}
