using NotesBackend.Models;

namespace NotesBackend.Repositories
{
    /// <summary>
    /// Repository abstraction for notes persistence.
    /// </summary>
    public interface INoteRepository
    {
        IEnumerable<Note> GetByUser(Guid userId);
        Note? GetById(Guid userId, Guid noteId);
        Note Add(Note note);
        bool Update(Note note);
        bool Delete(Guid userId, Guid noteId);
    }
}
