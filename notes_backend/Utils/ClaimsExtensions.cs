using System.Security.Claims;

namespace NotesBackend.Utils
{
    /// <summary>
    /// Claims helper extensions.
    /// </summary>
    public static class ClaimsExtensions
    {
        // PUBLIC_INTERFACE
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            /** Extracts the GUID user id from the JWT "sub" claim or throws if not present or invalid. */
            var sub = user.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new UnauthorizedAccessException("Invalid token: missing subject.");
            if (!Guid.TryParse(sub, out var id))
                throw new UnauthorizedAccessException("Invalid token: subject is not a GUID.");
            return id;
        }
    }
}
