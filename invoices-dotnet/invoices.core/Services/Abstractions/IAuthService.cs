using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;

namespace invoices.core.Services.Abstractions;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password, CancellationToken ct = default);
}
