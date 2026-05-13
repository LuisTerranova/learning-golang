using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;

namespace invoices.core.Services.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsAsync(string username, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
