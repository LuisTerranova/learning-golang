using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using invoices.core.Models;

namespace invoices.core.Services.Abstractions;

public interface IInvoiceService
{
    Task<List<Invoice>> GetAllAsync();
    Task GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(Guid id);

    Task SendInvoicesToProcessAsync(RawInvoice raw);
}