# Notes Backend (.NET 8)

A minimal ASP.NET Core Web API providing:
- JWT authentication (signup/login) — token validation parameters are derived from environment variables via JwtTokenService.
- CRUD for notes (create, list, get, update, delete)
- OpenAPI docs available at /docs

Environment variables (recommended to set via .env):
- JWT_ISSUER: JWT token issuer (default: notes-backend)
- JWT_AUDIENCE: JWT token audience (default: notes-clients)
- JWT_SECRET: HMAC secret for JWT signing (no default recommended for production; appsettings provides dev default)
- JWT_LIFETIME_MINUTES: Token lifetime in minutes (default from appsettings: 120)

Quick Start:
1) Set environment variables as needed (particularly JWT_SECRET for production).
2) Run the app (`dotnet run`) and open /docs to test endpoints.

Security notes:
- Passwords are hashed with SHA256 for demo purposes only. Replace with a strong password hashing scheme (PBKDF2, BCrypt, Argon2) in production.
- In-memory repositories are used for demo. Replace with a persistent data store for real-world use.
