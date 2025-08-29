using System.Security.Cryptography;
using System.Text;
using NotesBackend.Models;
using NotesBackend.Repositories;

namespace NotesBackend.Services
{
    /// <summary>
    /// Basic authentication logic including password hashing and validation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;

        public AuthService(IUserRepository users)
        {
            _users = users;
        }

        public User CreateUser(string username, string password)
        {
            if (_users.GetByUsername(username) != null)
                throw new InvalidOperationException("Username already exists");

            var user = new User
            {
                Username = username,
                PasswordHash = Hash(password)
            };
            _users.Add(user);
            return user;
        }

        public User? ValidateUser(string username, string password)
        {
            var user = _users.GetByUsername(username);
            if (user == null) return null;
            var hash = Hash(password);
            return user.PasswordHash == hash ? user : null;
        }

        private static string Hash(string text)
        {
            // For demo simplify with SHA256; replace with PBKDF2/BCrypt/Argon2 in production.
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(bytes);
        }
    }
}
