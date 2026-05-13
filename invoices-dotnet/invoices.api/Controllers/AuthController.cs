using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace invoices.api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService _authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthResult>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request.Username, request.Password, ct);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}

public record LoginRequest(string Username, string Password);
