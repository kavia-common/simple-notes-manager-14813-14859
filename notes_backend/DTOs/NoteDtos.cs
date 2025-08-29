using System.ComponentModel.DataAnnotations;

namespace NotesBackend.DTOs
{
    /// <summary>
    /// Create note request.
    /// </summary>
    public class CreateNoteRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Update note request.
    /// </summary>
    public class UpdateNoteRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Note response payload.
    /// </summary>
    public class NoteResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
