using System;
using System.Text.Json.Serialization;

namespace invoices.core.Models;

public class RawInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonIgnore]
    public string FileName { get; set; } = string.Empty;

    public byte[] ImageData { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public Invoice? ParsedInvoice { get; set; }
}
