using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FlowDesk.Domain.Entities;
using FlowDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FlowDesk.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", Register);
        app.MapPost("/api/auth/login",    Login);
        app.MapPost("/api/auth/oauth",    OAuthSignIn);
    }

    record OAuthRequest(string Provider, string ProviderKey, string Email, string? Name, string? AvatarUrl);

    record RegisterRequest(string Name, string Email, string Password);
    record LoginRequest(string Email, string Password);
    record AuthResponse(string AccessToken, string Email, string Name);

    static async Task<IResult> Register(
        RegisterRequest req, AppDbContext db, IConfiguration config)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email))
            return Results.Conflict(new { error = "Email already in use" });

        var user = new User
        {
            Name         = req.Name,
            Email        = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Results.Ok(new AuthResponse(GenerateToken(user, config), user.Email, user.Name));
    }

    static async Task<IResult> Login(
        LoginRequest req, AppDbContext db, IConfiguration config)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Results.Unauthorized();

        return Results.Ok(new AuthResponse(GenerateToken(user, config), user.Email, user.Name));
    }

    static async Task<IResult> OAuthSignIn(
        OAuthRequest req, AppDbContext db, IConfiguration config)
    {
        // Find existing user by email, or create a new one
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.ToLower());

        if (user is null)
        {
            user = new User
            {
                Email     = req.Email.ToLower(),
                Name      = req.Name ?? req.Email,
                AvatarUrl = req.AvatarUrl,
                // No PasswordHash for OAuth users
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        var token = GenerateToken(user, config);
        return Results.Ok(new { userId = user.Id, accessToken = token, email = user.Email, name = user.Name });
    }

    static string GenerateToken(User user, IConfiguration config)
    {
        var key    = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiryMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email),
            new Claim(ClaimTypes.Name,           user.Name),
        };

        var token = new JwtSecurityToken(
            issuer:              config["Jwt:Issuer"],
            audience:            config["Jwt:Audience"],
            claims:              claims,
            expires:             expiry,
            signingCredentials:  creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
