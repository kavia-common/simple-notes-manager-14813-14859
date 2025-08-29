using NotesBackend.Models;

namespace NotesBackend.Repositories
{
    /// <summary>
    /// Repository abstraction for user persistence.
    /// </summary>
    public interface IUserRepository
    {
        User? GetByUsername(string username);
        User? GetById(Guid id);
        void Add(User user);
    }
}
