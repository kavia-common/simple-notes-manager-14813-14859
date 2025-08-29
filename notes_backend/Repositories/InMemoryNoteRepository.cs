using NotesBackend.Models;

namespace NotesBackend.Repositories
{
    /// <summary>
    /// Simple in-memory notes repository for demo purposes.
    /// NOTE: Replace with database-backed implementation in production.
    /// </summary>
    public class InMemoryNoteRepository : INoteRepository
    {
        private readonly List<Note> _notes = new();

        public Note Add(Note note)
        {
            _notes.Add(note);
            return note;
        }

        public bool Delete(Guid userId, Guid noteId)
        {
            var note = _notes.FirstOrDefault(n => n.Id == noteId && n.UserId == userId);
            if (note == null) return false;
            _notes.Remove(note);
            return true;
        }

        public Note? GetById(Guid userId, Guid noteId)
        {
            return _notes.FirstOrDefault(n => n.Id == noteId && n.UserId == userId);
        }

        public IEnumerable<Note> GetByUser(Guid userId)
        {
            return _notes.Where(n => n.UserId == userId).OrderByDescending(n => n.UpdatedAtUtc);
        }

        public bool Update(Note note)
        {
            var existing = _notes.FirstOrDefault(n => n.Id == note.Id && n.UserId == note.UserId);
            if (existing == null) return false;
            existing.Title = note.Title;
            existing.Content = note.Content;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            return true;
        }
    }
}
