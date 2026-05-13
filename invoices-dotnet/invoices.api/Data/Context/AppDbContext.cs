using System;
using System.Collections.Generic;
using System.Linq;
using invoices.core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace invoices.api.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<RawInvoice> RawInvoices => Set<RawInvoice>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt)
                  .HasColumnType("timestamp with time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.RawText).HasColumnType("text");
            entity.Property(e => e.AccessKey).HasMaxLength(255);
            entity.Property(e => e.Establishment).HasMaxLength(200);
            entity.Property(e => e.Cnpj).HasMaxLength(20);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Date).HasColumnType("timestamp with time zone");
            entity.Property(e => e.ParserVersion).HasMaxLength(50);

            entity.Property(e => e.ParseErrors)
                  .HasColumnType("jsonb")
                  .HasConversion(
                      v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                      v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>()
                  )
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1!.SequenceEqual(c2!),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()
                  ));

            entity.HasIndex(e => e.AccessKey).IsUnique();
            entity.HasIndex(e => e.Cnpj);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.RawId);

            entity.OwnsMany(
                e => e.Items,
                item =>
                {
                    item.ToTable("InvoiceItems");
                    item.WithOwner().HasForeignKey("InvoiceId");
                    item.Property<int>("Id");
                    item.HasKey("Id");
                    item.Property(i => i.Name).HasMaxLength(200);
                    item.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
                    item.Property(i => i.Total).HasColumnType("decimal(18,2)");
                }
            );

            entity.HasOne(e => e.RawInvoice)
                  .WithOne(r => r.ParsedInvoice)
                  .HasForeignKey<Invoice>(e => e.RawId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RawInvoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ImageData).IsRequired();
            entity.Property(e => e.CreatedAt)
                  .HasColumnType("timestamp with time zone")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
