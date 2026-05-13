using System.Threading;
using System.Threading.Tasks;
using invoices.api.Data.Context;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace invoices.api.Data.Repositories;

public class UserRepository(AppDbContext _db) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<bool> ExistsAsync(string username, CancellationToken ct = default)
    {
        return await _db.Users.AnyAsync(u => u.Username == username, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _db.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
