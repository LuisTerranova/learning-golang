using invoices.core.Models;
using Microsoft.EntityFrameworkCore;

namespace invoices.api.Data.Context;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        db.Users.Add(
            new User
            {
                Username = "admin",
                Email = "admin@email.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );

        await db.SaveChangesAsync();
    }
}
