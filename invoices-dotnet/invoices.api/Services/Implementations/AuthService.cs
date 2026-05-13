using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace invoices.api.Services.Implementations;

public class AuthService(IUserRepository _userRepo, IConfiguration _config) : IAuthService
{

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByUsernameAsync(username, ct);
        if (user is null)
            return new AuthResult(false, null, null, "Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return new AuthResult(false, null, null, "Invalid credentials.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var expiration = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpirationInMinutes"]!));

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
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new AuthResult(true, tokenHandler.WriteToken(token), expiration, null);
    }
}
