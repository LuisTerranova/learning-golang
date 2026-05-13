using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.front.Services.Abstractions;

namespace invoices.front.Services.Implementations;

public class AuthService : IAuthClient
{
    private string? _token;
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public AuthService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5152") };
    }

    public string? Token => _token;
    public bool IsAuthenticated => _token is not null;

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var content = JsonContent.Create(new { username, password }, options: JsonOptions);
        var response = await _httpClient.PostAsync("api/auth/login", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions, ct);
            return error ?? new AuthResult(false, null, null, "Login failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions, ct);

        if (result?.Success == true)
        {
            _token = result.Token;
        }

        return result ?? new AuthResult(false, null, null, "Invalid server response.");
    }

    public void Logout()
    {
        _token = null;
    }
}
