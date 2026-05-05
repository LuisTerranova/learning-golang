using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.api.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    public Task<List<Invoice>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task SendInvoicesToProcessAsync(RawInvoice raw)
    {
        throw new NotImplementedException();
    }
}