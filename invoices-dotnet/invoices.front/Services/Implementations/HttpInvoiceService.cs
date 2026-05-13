using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.front.Services.Implementations;

public class HttpInvoiceService(HttpClient _httpClient) : IInvoiceService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<List<Invoice>> GetAllAsync(
        int page, int pageSize,
        string? search = null,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken ct = default)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            query += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(sortBy))
            query += $"&sortBy={Uri.EscapeDataString(sortBy)}&ascending={ascending.ToString().ToLowerInvariant()}";

        return await _httpClient.GetFromJsonAsync<List<Invoice>>($"api/invoices{query}", JsonOptions, ct) ?? new();
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _httpClient.GetFromJsonAsync<Invoice>($"api/invoices/{id}", JsonOptions, ct);
    }

    public async Task<int> GetCountAsync(string? search = null, CancellationToken ct = default)
    {
        var query = string.IsNullOrWhiteSpace(search) ? "" : $"?search={Uri.EscapeDataString(search)}";
        return await _httpClient.GetFromJsonAsync<int>($"api/invoices/count{query}", JsonOptions, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/invoices/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        var content = JsonContent.Create(invoice, options: JsonOptions);
        var response = await _httpClient.PutAsync($"api/invoices/{invoice.Id}", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendInvoicesToProcessAsync(RawInvoice raw, CancellationToken ct = default)
    {
        var content = JsonContent.Create(raw, options: JsonOptions);
        var response = await _httpClient.PostAsync("api/invoices/process", content, ct);
        response.EnsureSuccessStatusCode();
    }
}
