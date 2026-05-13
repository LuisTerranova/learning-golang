using System.Text;
using System.Text.Json;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using RabbitMQ.Client;

namespace invoices.api.Services.Implementations;

public class InvoiceService(
    IInvoiceRepository repo,
    IChannel channel,
    JsonSerializerOptions jsonOptions
) : IInvoiceService
{
    public async Task<List<Invoice>> GetAllAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        bool ascending = false,
        CancellationToken ct = default
    )
    {
        return await repo.GetAllAsync(page, pageSize, search, sortBy, ascending, ct);
    }

    public async Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await repo.GetByIdAsync(id, ct);
    }

    public async Task<int> GetCountAsync(string? search = null, CancellationToken ct = default)
    {
        return await repo.GetCountAsync(search, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await repo.GetByIdAsync(id, ct);

        if (invoice is null)
            throw new KeyNotFoundException($"Invoice with id '{id}' was not found.");

        await repo.DeleteAsync(invoice, ct);
    }

    public async Task UpdateAsync(Invoice invoice, CancellationToken ct = default)
    {
        if (!await repo.ExistsAsync(invoice.Id, ct))
            throw new KeyNotFoundException($"Invoice with id '{invoice.Id}' was not found.");

        await repo.UpdateAsync(invoice, ct);
    }

    public async Task SendInvoicesToProcessAsync(RawInvoice raw, CancellationToken ct = default)
    {
        await repo.AddRawInvoiceAsync(raw, ct);
        await repo.SaveChangesAsync(ct);

        await channel.QueueDeclareAsync(
            queue: "invoices_to_process",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: ct
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(raw, jsonOptions));

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "invoices_to_process",
            body: body,
            cancellationToken: ct
        );
    }
}
