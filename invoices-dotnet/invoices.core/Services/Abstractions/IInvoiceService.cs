using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;

namespace invoices.core.Services.Abstractions;

public interface IInvoiceService
{
    Task<List<Invoice>> GetAllAsync(
        int page, int pageSize,
        string? search = null,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken ct = default);

    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> GetCountAsync(string? search = null, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);

    Task SendInvoicesToProcessAsync(RawInvoice raw, CancellationToken ct = default);
}