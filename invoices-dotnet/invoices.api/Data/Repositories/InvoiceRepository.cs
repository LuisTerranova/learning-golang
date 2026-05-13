using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using invoices.api.Data.Context;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace invoices.api.Data.Repositories;

public class InvoiceRepository(AppDbContext db) : IInvoiceRepository
{
    private static readonly Dictionary<string, Expression<Func<Invoice, object>>> SortExpressions =
        new()
        {
            [nameof(Invoice.Date)] = i => i.Date ?? DateTimeOffset.MinValue,
            [nameof(Invoice.Total)] = i => i.Total ?? 0m,
            [nameof(Invoice.Establishment)] = i => i.Establishment ?? string.Empty,
            [nameof(Invoice.Cnpj)] = i => i.Cnpj ?? string.Empty,
            ["createdAt"] = i => i.RawInvoice!.CreatedAt,
        };

    public async Task<List<Invoice>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken ct = default
    )
    {
        var query = BuildSearchQuery(db.Invoices.Include(i => i.Items).AsNoTracking(), search);

        query = ApplySorting(query, sortBy, ascending);

        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<int> GetCountAsync(string? search = null, CancellationToken ct = default)
    {
        var query = BuildSearchQuery(db.Invoices.AsNoTracking(), search);
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken ct = default)
    {
        await db.Invoices.AddAsync(invoice, ct);
    }

    public async Task AddRangeAsync(IEnumerable<Invoice> invoices, CancellationToken ct = default)
    {
        await db.Invoices.AddRangeAsync(invoices, ct);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        db.Invoices.Update(invoice);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Invoice invoice, CancellationToken ct = default)
    {
        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync(ct);
    }

    public async Task AddRawInvoiceAsync(RawInvoice rawInvoice, CancellationToken ct = default)
    {
        await db.RawInvoices.AddAsync(rawInvoice, ct);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Invoices.AnyAsync(i => i.Id == id, ct);
    }

    public async Task<RawInvoice?> GetRawInvoiceByInvoiceIdAsync(
        Guid invoiceId,
        CancellationToken ct = default
    )
    {
        return await db
            .Invoices.Where(i => i.Id == invoiceId)
            .Select(i => i.RawInvoice)
            .FirstOrDefaultAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
    }

    private static IQueryable<Invoice> BuildSearchQuery(IQueryable<Invoice> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim().ToLowerInvariant();
        return query.Where(i =>
            (i.Establishment != null && i.Establishment.ToLower().Contains(term))
            || (i.Cnpj != null && i.Cnpj.Contains(term))
            || (i.AccessKey != null && i.AccessKey.Contains(term))
            || (i.RawText != null && i.RawText.ToLower().Contains(term))
        );
    }

    private static IQueryable<Invoice> ApplySorting(
        IQueryable<Invoice> query,
        string? sortBy,
        bool ascending
    )
    {
        if (string.IsNullOrWhiteSpace(sortBy) || !SortExpressions.TryGetValue(sortBy, out var expr))
            return query.OrderByDescending(i => i.Date ?? DateTimeOffset.MinValue);

        return ascending ? query.OrderBy(expr) : query.OrderByDescending(expr);
    }
}
