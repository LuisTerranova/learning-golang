using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using RabbitMQ.Client;

namespace invoices.api.Services.Implementations;

public class InvoiceService(IInvoiceRepository _repo, IChannel _channel, JsonSerializerOptions _jsonOptions) : IInvoiceService
{
    public async Task<List<Invoice>> GetAllAsync(
        int page, int pageSize,
        string? search = null,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken ct = default)
    {
        return await _repo.GetAllAsync(page, pageSize, search, sortBy, ascending, ct);
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.GetByIdAsync(id, ct);
    }

    public async Task<int> GetCountAsync(string? search = null, CancellationToken ct = default)
    {
        return await _repo.GetCountAsync(search, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _repo.GetByIdAsync(id, ct);

        if (invoice is null)
            throw new KeyNotFoundException($"Invoice with id '{id}' was not found.");

        await _repo.DeleteAsync(invoice, ct);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        if (!await _repo.ExistsAsync(invoice.Id, ct))
            throw new KeyNotFoundException($"Invoice with id '{invoice.Id}' was not found.");

        await _repo.UpdateAsync(invoice, ct);
    }

    public async Task SendInvoicesToProcessAsync(RawInvoice raw, CancellationToken ct = default)
    {
        await _repo.AddRawInvoiceAsync(raw, ct);
        await _repo.SaveChangesAsync(ct);

        await _channel.QueueDeclareAsync(
            queue: "invoices_to_process",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(raw, _jsonOptions));

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "invoices_to_process",
            body: body,
            cancellationToken: ct
        );
    }

}
