using NotesBackend.DTOs;
using NotesBackend.Models;

namespace NotesBackend.Services
{
    /// <summary>
    /// Authentication service abstraction.
    /// </summary>
    public interface IAuthService
    {
        User? ValidateUser(string username, string password);
        User CreateUser(string username, string password);
    }
}
