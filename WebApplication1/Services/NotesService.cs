using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models.Requests;
using WebApplication1.Models.Responses;

namespace WebApplication1.Services
{
    public class NotesService
    {
        private readonly AppDbContext _context;

        public NotesService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Helper: Extract UserId from JWT Claims
        public int GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                throw new UnauthorizedAccessException("Missing userId claim in token.");

            if (!int.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException("Invalid userId claim in token.");

            return userId;
        }

        // ✅ 1) Create Note (for current user)
        public NoteResponse CreateNote(int userId, CreateNoteRequest request)
        {
            var note = new Note
            {
                Title = request.Title,
                Content = request.Content,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notes.Add(note);
            _context.SaveChanges();

            return new NoteResponse
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                CreatedAt = note.CreatedAt
            };
        }

        // ✅ 2) Get all notes for current user
        public List<NoteResponse> GetMyNotes(int userId)
        {
            return _context.Notes
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NoteResponse
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt
                })
                .ToList();
        }

        // ✅ 3) Get single note by id (ownership enforced)
        public NoteResponse? GetMyNoteById(int userId, int noteId)
        {
            return _context.Notes
                .Where(n => n.Id == noteId && n.UserId == userId)
                .Select(n => new NoteResponse
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    CreatedAt = n.CreatedAt
                })
                .FirstOrDefault();
        }

        // ✅ 4) Update note (ownership enforced)
        public bool UpdateMyNote(int userId, int noteId, UpdateNoteRequest request)
        {
            var note = _context.Notes
                .FirstOrDefault(n => n.Id == noteId && n.UserId == userId);

            if (note == null)
                return false;

            note.Title = request.Title;
            note.Content = request.Content;

            _context.SaveChanges();
            return true;
        }

        // ✅ 5) Delete note (ownership enforced)
        public bool DeleteMyNote(int userId, int noteId)
        {
            var note = _context.Notes
                .FirstOrDefault(n => n.Id == noteId && n.UserId == userId);

            if (note == null)
                return false;

            _context.Notes.Remove(note);
            _context.SaveChanges();
            return true;
        }
    }
}
