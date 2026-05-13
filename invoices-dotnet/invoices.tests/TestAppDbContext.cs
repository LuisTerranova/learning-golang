using System.Text.Json;
using invoices.api.Data.Context;
using invoices.core.Models;
using Microsoft.EntityFrameworkCore;

namespace invoices.tests;

public class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(e => e.ParseErrors)
                  .HasColumnType("TEXT")
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new()
                  );
        });

        modelBuilder.Entity<RawInvoice>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasColumnType("TEXT");
        });
    }
}
