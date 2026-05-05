using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using frontend_avalonia.Models;

namespace frontend_avalonia.Services.Abstractions;

public interface IInvoiceService
{
    Task<List<Invoice>> GetAllAsync();
    Task GetByIdAsync(Guid id);
    Task DeleteAsync(Guid id);
    Task UpdateAsync(Guid id);

    Task SendInvoicesToProcessAsync(RawInvoice raw);
}