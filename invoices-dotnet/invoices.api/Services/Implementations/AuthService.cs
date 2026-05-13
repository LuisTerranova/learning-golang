using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using invoices.api.Data.Context;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace invoices.api.Services.Implementations;

public class AuthService(IUserRepository userRepo, IConfiguration config, AppDbContext db)
    : IAuthService
{
    public async Task<AuthResult> LoginAsync(
        string username,
        string password,
        CancellationToken ct = default
    )
    {
        var user = await userRepo.GetByUsernameAsync(username, ct);
        if (user is null)
            return new AuthResult(false, null, null, null, "Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return new AuthResult(false, null, null, null, "Invalid credentials.");

        var jwt = GenerateJwt(user, out var expiration);
        var refreshToken = GenerateRefreshToken(user.Id);

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(ct);

        return new AuthResult(true, jwt, refreshToken.Token, expiration, null);
    }

    public async Task<AuthResult> RefreshAsync(string token, CancellationToken ct = default)
    {
        var storedToken = await db
            .RefreshTokens.Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

        if (
            storedToken is null
            || storedToken.IsRevoked
            || storedToken.ExpiresAt <= DateTime.UtcNow
        )
        {
            return new AuthResult(false, null, null, null, "Invalid or expired refresh token.");
        }

        // Generate new JWT
        var jwt = GenerateJwt(storedToken.User, out var expiration);

        // Generate new refresh token to rotate
        var newRefreshToken = GenerateRefreshToken(storedToken.UserId);

        // Revoke the old one
        storedToken.IsRevoked = true;
        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(ct);

        return new AuthResult(true, jwt, newRefreshToken.Token, expiration, null);
    }

    public async Task LogoutAsync(string token, CancellationToken ct = default)
    {
        var storedToken = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, ct);

        if (storedToken is not null)
        {
            storedToken.IsRevoked = true;
            await db.SaveChangesAsync(ct);
        }
    }

    private string GenerateJwt(User user, out DateTime expiration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
        expiration = DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpirationInMinutes"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            ),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(Guid userId)
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
        };
    }
}
