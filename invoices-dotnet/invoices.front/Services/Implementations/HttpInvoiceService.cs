using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.front.Services.Implementations;

public class HttpInvoiceService(HttpClient _httpClient) : IInvoiceService
{
    public async Task<List<Invoice>> GetAllAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Invoice>>("api/invoices") ?? new();
    }

    public async Task GetByIdAsync(Guid id)
    {
        await _httpClient.GetAsync($"api/invoices/{id}");
    }

    public async Task DeleteAsync(Guid id)
    {
        await _httpClient.DeleteAsync($"api/invoices/{id}");
    }

    public async Task UpdateAsync(Guid id)
    {
        await _httpClient.PutAsync($"api/invoices/{id}", null);
    }

    public async Task SendInvoicesToProcessAsync(RawInvoice raw)
    {
        await _httpClient.PostAsJsonAsync("api/invoices/process", raw);
    }
}
