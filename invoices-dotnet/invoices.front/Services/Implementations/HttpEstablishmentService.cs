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

public class HttpEstablishmentService(HttpClient httpClient) : IEstablishmentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<List<Establishment>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<Establishment>();
        
        return await httpClient.GetFromJsonAsync<List<Establishment>>($"api/establishments/search?q={Uri.EscapeDataString(query)}", JsonOptions, ct) ?? new();
    }
}
