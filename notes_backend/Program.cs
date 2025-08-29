using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NotesBackend.DTOs;
using NotesBackend.Models;
using NotesBackend.Repositories;
using NotesBackend.Services;
using NotesBackend.Utils;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenAPI/Swagger with NSwag
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "Notes API";
    settings.Description = "REST API for user authentication and note management.";
    settings.Version = "v1";
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Dependency Injection: Repositories and Services
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<INoteRepository, InMemoryNoteRepository>();
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

/* JWT Authentication configuration (from env vars or appsettings)
   Configure TokenValidationParameters directly using configuration to avoid creating an extra ServiceProvider inside Program.cs. */
var jwtIssuer = Environment.GetEnvironmentVariable(JwtTokenService.EnvJwtIssuer) ?? builder.Configuration["Jwt:Issuer"] ?? "notes-backend";
var jwtAudience = Environment.GetEnvironmentVariable(JwtTokenService.EnvJwtAudience) ?? builder.Configuration["Jwt:Audience"] ?? "notes-clients";
var jwtSecret = Environment.GetEnvironmentVariable(JwtTokenService.EnvJwtSecret) ?? builder.Configuration["Jwt:Secret"] ?? "CHANGE_ME_DEV_SECRET_32CHARS_MINIMUM";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // enable HTTPS in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// OpenAPI/Swagger
app.UseOpenApi();
app.UseSwaggerUi(config => { config.Path = "/docs"; });

// Root and health
// PUBLIC_INTERFACE
app.MapGet("/", () => new { message = "Healthy" })
   .WithName("HealthCheck")
   .WithTags("System")
   .WithSummary("Health check")
   .WithDescription("Returns a simple object to indicate the service is running.");

// Auth endpoints
// PUBLIC_INTERFACE
app.MapPost("/api/auth/signup",
    (SignupRequest req, IAuthService auth, ITokenService tokens) =>
    {
        /** Creates a new user and returns a JWT token for immediate use. */
        if (req == null) return Results.BadRequest(new { error = "Invalid request" });

        try
        {
            var user = auth.CreateUser(req.Username, req.Password);
            var expires = DateTime.UtcNow.Add(tokens.GetDefaultLifetime());
            var token = tokens.CreateToken(user, expires);
            return Results.Ok(new LoginResponse
            {
                Token = token,
                ExpiresAtUtc = expires
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    })
   .WithName("Signup")
   .WithTags("Auth")
   .WithSummary("User signup")
   .WithDescription("Create a new user account and receive a JWT for authentication.")
   .Accepts<SignupRequest>("application/json")
   .Produces<LoginResponse>(StatusCodes.Status200OK)
   .Produces(StatusCodes.Status400BadRequest)
   .Produces(StatusCodes.Status409Conflict);

// PUBLIC_INTERFACE
app.MapPost("/api/auth/login",
    (LoginRequest req, IAuthService auth, ITokenService tokens) =>
    {
        /** Authenticates a user and returns a JWT token on success. */
        if (req == null) return Results.BadRequest(new { error = "Invalid request" });

        var user = auth.ValidateUser(req.Username, req.Password);
        if (user == null) return Results.Unauthorized();

        var expires = DateTime.UtcNow.Add(tokens.GetDefaultLifetime());
        var token = tokens.CreateToken(user, expires);

        return Results.Ok(new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expires
        });
    })
   .WithName("Login")
   .WithTags("Auth")
   .WithSummary("User login")
   .WithDescription("Authenticate and receive a JWT.")
   .Accepts<LoginRequest>("application/json")
   .Produces<LoginResponse>(StatusCodes.Status200OK)
   .Produces(StatusCodes.Status401Unauthorized)
   .Produces(StatusCodes.Status400BadRequest);

// Notes endpoints (require auth)
var notesGroup = app.MapGroup("/api/notes")
                    .RequireAuthorization()
                    .WithTags("Notes");

// PUBLIC_INTERFACE
notesGroup.MapGet("/",
    (ClaimsPrincipal principal, INoteRepository repo) =>
    {
        /** List all notes for the authenticated user. */
        var userId = principal.GetUserId();
        var items = repo.GetByUser(userId)
                        .Select(n => new NoteResponse
                        {
                            Id = n.Id,
                            Title = n.Title,
                            Content = n.Content,
                            CreatedAtUtc = n.CreatedAtUtc,
                            UpdatedAtUtc = n.UpdatedAtUtc
                        });
        return Results.Ok(items);
    })
    .WithName("ListNotes")
    .WithSummary("List notes")
    .WithDescription("Returns all notes for the authenticated user.")
    .Produces<IEnumerable<NoteResponse>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// PUBLIC_INTERFACE
notesGroup.MapGet("/{id:guid}",
    (Guid id, ClaimsPrincipal principal, INoteRepository repo) =>
    {
        /** Get a single note by id for the authenticated user. */
        var userId = principal.GetUserId();
        var note = repo.GetById(userId, id);
        if (note == null) return Results.NotFound();
        return Results.Ok(new NoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            CreatedAtUtc = note.CreatedAtUtc,
            UpdatedAtUtc = note.UpdatedAtUtc
        });
    })
    .WithName("GetNote")
    .WithSummary("Get note")
    .WithDescription("Returns a note by id for the authenticated user.")
    .Produces<NoteResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

// PUBLIC_INTERFACE
notesGroup.MapPost("/",
    (CreateNoteRequest req, ClaimsPrincipal principal, INoteRepository repo) =>
    {
        /** Create a note for the authenticated user. */
        if (req == null) return Results.BadRequest(new { error = "Invalid request" });
        var userId = principal.GetUserId();

        var note = new Note
        {
            UserId = userId,
            Title = req.Title,
            Content = req.Content,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        repo.Add(note);

        var resp = new NoteResponse
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            CreatedAtUtc = note.CreatedAtUtc,
            UpdatedAtUtc = note.UpdatedAtUtc
        };
        return Results.Created($"/api/notes/{note.Id}", resp);
    })
    .WithName("CreateNote")
    .WithSummary("Create note")
    .WithDescription("Creates a new note for the authenticated user.")
    .Accepts<CreateNoteRequest>("application/json")
    .Produces<NoteResponse>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized);

// PUBLIC_INTERFACE
notesGroup.MapPut("/{id:guid}",
    (Guid id, UpdateNoteRequest req, ClaimsPrincipal principal, INoteRepository repo) =>
    {
        /** Update an existing note. */
        if (req == null) return Results.BadRequest(new { error = "Invalid request" });

        var userId = principal.GetUserId();
        var existing = repo.GetById(userId, id);
        if (existing == null) return Results.NotFound();

        existing.Title = req.Title;
        existing.Content = req.Content;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        repo.Update(existing);

        var resp = new NoteResponse
        {
            Id = existing.Id,
            Title = existing.Title,
            Content = existing.Content,
            CreatedAtUtc = existing.CreatedAtUtc,
            UpdatedAtUtc = existing.UpdatedAtUtc
        };
        return Results.Ok(resp);
    })
    .WithName("UpdateNote")
    .WithSummary("Update note")
    .WithDescription("Updates an existing note by id for the authenticated user.")
    .Accepts<UpdateNoteRequest>("application/json")
    .Produces<NoteResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized);

// PUBLIC_INTERFACE
notesGroup.MapDelete("/{id:guid}",
    (Guid id, ClaimsPrincipal principal, INoteRepository repo) =>
    {
        /** Delete a note by id for the authenticated user. */
        var userId = principal.GetUserId();
        var ok = repo.Delete(userId, id);
        return ok ? Results.NoContent() : Results.NotFound();
    })
    .WithName("DeleteNote")
    .WithSummary("Delete note")
    .WithDescription("Deletes a note by id for the authenticated user.")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

// Provide a small docs endpoint to document WebSocket or real-time usage (not applicable here but per guidelines)
app.MapGet("/api/docs/websocket", () =>
{
    /** This API currently does not expose WebSockets. */
    return Results.Ok(new
    {
        message = "No WebSocket endpoints are available in this service."
    });
})
.WithName("WebSocketHelp")
.WithTags("System")
.WithSummary("WebSocket usage help")
.WithDescription("Provides guidance on real-time/WebSocket usage (none in this project).");

app.Run();