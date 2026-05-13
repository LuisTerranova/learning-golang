using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.front.Services.Abstractions;

namespace invoices.front.Services.Implementations;

public class AuthService : IAuthClient
{
    private byte[]? _tokenBytes;
    private byte[]? _refreshTokenBytes;
    private readonly HttpClient _httpClient;
    private CancellationTokenSource? _refreshCts;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public AuthService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7118") };
    }

    public string? Token => _tokenBytes != null ? Encoding.UTF8.GetString(_tokenBytes) : null;
    private string? RefreshToken => _refreshTokenBytes != null ? Encoding.UTF8.GetString(_refreshTokenBytes) : null;

    public bool IsAuthenticated => _tokenBytes is not null;

    public async Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var content = JsonContent.Create(new { username, password }, options: JsonOptions);
        var response = await _httpClient.PostAsync("api/auth/login", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions, ct);
            return error ?? new AuthResult(false, null, null, null, "Login failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions, ct);

        if (result?.Success == true && result.Token != null && result.RefreshToken != null)
        {
            _tokenBytes = Encoding.UTF8.GetBytes(result.Token);
            _refreshTokenBytes = Encoding.UTF8.GetBytes(result.RefreshToken);
            ScheduleProactiveRefresh();
        }

        return result ?? new AuthResult(false, null, null, null, "Invalid server response.");
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var content = JsonContent.Create(new { refreshToken }, options: JsonOptions);
        var response = await _httpClient.PostAsync("api/auth/refresh", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            Logout();
            return new AuthResult(false, null, null, null, "Refresh failed.");
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(JsonOptions, ct);

        if (result?.Success == true && result.Token != null && result.RefreshToken != null)
        {
            _tokenBytes = Encoding.UTF8.GetBytes(result.Token);
            _refreshTokenBytes = Encoding.UTF8.GetBytes(result.RefreshToken);
            ScheduleProactiveRefresh();
        }

        return result ?? new AuthResult(false, null, null, null, "Invalid server response.");
    }

    public async Task<bool> TryRefreshAsync(CancellationToken ct = default)
    {
        var rt = RefreshToken;
        if (rt == null) return false;

        var result = await RefreshAsync(rt, ct);
        return result.Success;
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var content = JsonContent.Create(new { refreshToken }, options: JsonOptions);
            await _httpClient.PostAsync("api/auth/logout", content, ct);
        }
        Logout();
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var rt = RefreshToken;
        if (!string.IsNullOrEmpty(rt))
        {
            var content = JsonContent.Create(new { refreshToken = rt }, options: JsonOptions);
            await _httpClient.PostAsync("api/auth/logout", content, ct);
        }
        Logout();
    }

    public void Logout()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = null;

        if (_tokenBytes != null)
        {
            Array.Clear(_tokenBytes, 0, _tokenBytes.Length);
            _tokenBytes = null;
        }

        if (_refreshTokenBytes != null)
        {
            Array.Clear(_refreshTokenBytes, 0, _refreshTokenBytes.Length);
            _refreshTokenBytes = null;
        }
    }

    private void ScheduleProactiveRefresh()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();

        var token = Token;
        if (token == null) return;

        var exp = GetTokenExpiry(token);
        if (exp == null) return;

        var now = DateTimeOffset.UtcNow;
        var lifetime = exp.Value - now;
        if (lifetime <= TimeSpan.Zero) return;

        // Refresh at 80% of the token's lifetime
        var delay = lifetime * 0.8;

        _refreshCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, _refreshCts.Token);
                if (!_refreshCts.Token.IsCancellationRequested)
                {
                    await TryRefreshAsync(_refreshCts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    private static DateTimeOffset? GetTokenExpiry(string token)
    {
        var parts = token.Split('.');
        if (parts.Length < 2) return null;

        var payload = parts[1];
        var padded = (payload.Length % 4) switch
        {
            2 => payload + "==",
            3 => payload + "=",
            _ => payload
        };

        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var expProp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(expProp.GetInt64());
            }
        }
        catch
        {
        }

        return null;
    }
}
