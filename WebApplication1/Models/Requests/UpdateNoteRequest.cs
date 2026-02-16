using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Requests
{
    public class UpdateNoteRequest
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }
    }
}
