using NotesBackend.Models;

namespace NotesBackend.Services
{
    /// <summary>
    /// JWT token generation abstraction.
    /// </summary>
    public interface ITokenService
    {
        string CreateToken(User user, DateTime expiresAtUtc);
        TimeSpan GetDefaultLifetime();
    }
}
