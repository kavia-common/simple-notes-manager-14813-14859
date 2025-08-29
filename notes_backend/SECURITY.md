# Security Notes

- Ensure JWT_SECRET is a strong, randomly generated value (at least 32 chars). Do not commit secrets.
- In production, enable HTTPS and set JwtBearerOptions.RequireHttpsMetadata = true.
- Replace demo SHA256 password hashing with PBKDF2/BCrypt/Argon2.
- Replace in-memory repositories with persistent storage (e.g., EF Core) and enable per-user data scoping.
- Configure CORS to only allow trusted origins.
