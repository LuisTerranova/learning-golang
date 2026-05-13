using invoices.core.Services.Abstractions;

namespace invoices.front.Services.Abstractions;

public interface IAuthClient : IAuthService
{
    string? Token { get; }
    bool IsAuthenticated { get; }
    void Logout();
}
