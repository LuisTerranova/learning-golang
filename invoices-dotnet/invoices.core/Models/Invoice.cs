using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace invoices.core.Models;

public class Invoice
{
    public Guid Id { get; set; }

    public Guid RawId { get; set; }

    [JsonIgnore]
    public RawInvoice? RawInvoice { get; set; }

    public string? RawText { get; set; }

    public string? AccessKey { get; set; }

    public string? Establishment { get; set; }

    public string? Cnpj { get; set; }

    public DateTimeOffset? Date { get; set; }

    public decimal? Total { get; set; }

    public List<ParsedItem>? Items { get; set; }

    public string ParserVersion { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    public List<string>? ParseErrors { get; set; }
}
