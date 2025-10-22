// Program.cs (ASP.NET Core minimal API)
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SafeVault.Api.Services;
using SafeVault.Api.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InputSanitizer>();
builder.Services.AddScoped<UserRepository>(); // register repository (uses connection string)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

var sessions = new Dictionary<string, string>(); // token => role

app.MapPost("/submit", async (UserDto user, InputSanitizer sanitizer, UserRepository repo) =>
{
    // Server-side validation using DataAnnotations
    var validationContext = new ValidationContext(user);
    var results = new List<ValidationResult>();
    if (!Validator.TryValidateObject(user, validationContext, results, true))
    {
        return Results.BadRequest(results.Select(r => r.ErrorMessage));
    }

    // Sanitize inputs
    user.Username = sanitizer.SanitizeString(user.Username, maxLength: 100);
    user.Email = sanitizer.SanitizeEmail(user.Email);

    // Save safely (parameterized)
    var userId = await repo.InsertUserAsync(user);
    return Results.Ok(new { UserId = userId });
});

app.MapPost("/login", async (UserAuthService auth, string username, string password) =>
{
    bool valid = await auth.AuthenticateUserAsync(username, password);
    if (!valid) return Results.Unauthorized();

    var user = await auth._repo.GetUserByUsernameAsync(username);
    string token = Guid.NewGuid().ToString();
    sessions[token] = user?.Role ?? "user";
    return Results.Ok(new { Token = token, Role = user?.Role });
});

app.MapGet("/admin", (HttpContext context) =>
{
    if (!context.Request.Headers.TryGetValue("Authorization", out var token))
        return Results.Unauthorized();

    if (!sessions.TryGetValue(token!, out var role) || role != "admin")
        return Results.Forbid();

    return Results.Ok("Welcome, Admin!");
});

app.Run();
