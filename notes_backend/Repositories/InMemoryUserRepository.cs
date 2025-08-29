using NotesBackend.Models;

namespace NotesBackend.Repositories
{
    /// <summary>
    /// Simple in-memory user repository for demo purposes.
    /// NOTE: Replace with database-backed implementation in production.
    /// </summary>
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public void Add(User user)
        {
            _users.Add(user);
        }

        public User? GetById(Guid id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public User? GetByUsername(string username)
        {
            return _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }
    }
}
