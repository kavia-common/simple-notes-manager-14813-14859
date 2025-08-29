using System.ComponentModel.DataAnnotations;

namespace NotesBackend.DTOs
{
    /// <summary>
    /// Signup request payload.
    /// </summary>
    public class SignupRequest
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login request payload.
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login response payload with JWT token.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAtUtc { get; set; }
    }
}
