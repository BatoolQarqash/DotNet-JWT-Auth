using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.Requests;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ✅ كل endpoints هنا تحتاج JWT
    public class NotesController : ControllerBase
    {
        private readonly NotesService _notesService;

        public NotesController(NotesService notesService)
        {
            _notesService = notesService;
        }

        // ===================== CREATE NOTE =====================
        [HttpPost]
        public IActionResult Create(CreateNoteRequest request)
        {
            // ✅ 1) نستخرج userId من JWT Claims
            var userId = _notesService.GetUserIdFromClaims(User);

            // ✅ 2) ننشئ note مرتبطة بهذا المستخدم
            var created = _notesService.CreateNote(userId, request);

            // ✅ 3) نرجع 201 Created + note response
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ===================== GET MY NOTES =====================
        [HttpGet]
        public IActionResult GetMyNotes()
        {
            var userId = _notesService.GetUserIdFromClaims(User);
            var notes = _notesService.GetMyNotes(userId);
            return Ok(notes);
        }

        // ===================== GET NOTE BY ID =====================
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var userId = _notesService.GetUserIdFromClaims(User);
            var note = _notesService.GetMyNoteById(userId, id);

            if (note == null)
                return NotFound(new { message = "Note not found." });

            return Ok(note);
        }

        // ===================== UPDATE NOTE =====================
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, UpdateNoteRequest request)
        {
            var userId = _notesService.GetUserIdFromClaims(User);
            var updated = _notesService.UpdateMyNote(userId, id, request);

            if (!updated)
                return NotFound(new { message = "Note not found." });

            return Ok(new { message = "Note updated successfully." });
        }

        // ===================== DELETE NOTE =====================
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var userId = _notesService.GetUserIdFromClaims(User);
            var deleted = _notesService.DeleteMyNote(userId, id);

            if (!deleted)
                return NotFound(new { message = "Note not found." });

            return Ok(new { message = "Note deleted successfully." });
        }
    }
}
