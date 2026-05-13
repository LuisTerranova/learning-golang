using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using invoices.api.Data.Context;
using invoices.core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace invoices.tests.Controllers;

public class AuthControllerTests : IClassFixture<TestFactory>
{
    private readonly HttpClient _client;
    private readonly TestFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerTests(TestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
    }

    private async Task SeedAdminAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedData.InitializeAsync(db);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WithValidCredentials()
    {
        await SeedAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin@123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WithWrongPassword()
    {
        await SeedAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "wrong" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var result = await response.Content.ReadFromJsonAsync<AuthResult>(_jsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task Login_ShouldReturn401_WithUnknownUser()
    {
        await SeedAdminAsync();

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "nobody", password = "anything" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnOk_EvenWithNoExplicitSeed_BecauseStartupSeeds()
    {
        // SeedData.InitializeAsync runs during app startup in Program.cs,
        // so the admin user already exists even without explicit seeding.
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin@123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
