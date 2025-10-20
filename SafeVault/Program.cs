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

app.Run();
