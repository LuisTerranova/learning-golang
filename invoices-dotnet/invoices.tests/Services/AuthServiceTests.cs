using invoices.api.Services.Implementations;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.Extensions.Configuration;

namespace invoices.tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>(MockBehavior.Strict);

        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "f82d3a1b9c7e4f6a8b0d2c5e3f7a1b9c8d0e2f4a6b8c0d2e4f6a8b0c2d4e6f8",
            ["Jwt:Issuer"] = "invoices-api",
            ["Jwt:Audience"] = "invoices-front",
            ["Jwt:ExpirationInMinutes"] = "60",
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _sut = new AuthService(_mockUserRepo.Object, _config);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync("unknown", "password");

        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
        result.ErrorMessage.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsWrong()
    {
        var user = new User
        {
            Username = "admin",
            Email = "admin@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync("admin", "WrongPassword");

        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
        result.ErrorMessage.Should().Be("Invalid credentials.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WithValidCredentials()
    {
        var user = new User
        {
            Username = "admin",
            Email = "admin@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync("admin", "Admin@123");

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().NotBeNull();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnValidJwtToken()
    {
        var user = new User
        {
            Username = "admin",
            Email = "admin@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        };

        _mockUserRepo
            .Setup(r => r.GetByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync("admin", "Admin@123");

        // JWT tokens have 3 dot-separated parts: header.payload.signature
        result.Token.Should().NotBeNull();
        result.Token!.Split('.').Should().HaveCount(3);
    }
}
