using invoices.core.Models;
using Microsoft.EntityFrameworkCore;

namespace invoices.api.Data.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<RawInvoice> RawInvoices => Set<RawInvoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

            entity.OwnsMany(
                e => e.Items,
                item =>
                {
                    item.ToTable("InvoiceItems");
                    item.WithOwner().HasForeignKey("InvoiceId");
                    item.Property<int>("Id");
                    item.HasKey("Id");
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
            entity.Property(e => e.ImageData).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
