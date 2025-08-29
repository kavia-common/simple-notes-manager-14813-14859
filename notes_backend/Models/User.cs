using System.ComponentModel.DataAnnotations;

namespace NotesBackend.Models
{
    /// <summary>
    /// Represents an application user.
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        // Stored as a hashed password (simplified for demo; use proper hashing in production)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
