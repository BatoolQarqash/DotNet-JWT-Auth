using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminCategoriesController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _db.Categories
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var name = request.Name.Trim();

            var exists = await _db.Categories.AnyAsync(c => c.Name == name);
            if (exists) return BadRequest("Category name already exists.");

            var category = new Category { Name = name };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return Ok(new { category.Id, category.Name });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var name = request.Name.Trim();

            var exists = await _db.Categories.AnyAsync(c => c.Id != id && c.Name == name);
            if (exists) return BadRequest("Category name already exists.");

            category.Name = name;
            await _db.SaveChangesAsync();

            return Ok(new { category.Id, category.Name });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // بما إنك حاطة Restrict، الحذف رح يفشل إذا عليها Posts
            _db.Categories.Remove(category);

            try
            {
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch
            {
                return BadRequest("Cannot delete category because it has posts.");
            }
        }
    }

    public record CreateCategoryRequest(string Name);
    public record UpdateCategoryRequest(string Name);
}
