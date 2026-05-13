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

    public Guid? EstablishmentId { get; set; }

    [JsonPropertyName("establishment_details")]
    public Establishment? Establishment { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    [JsonPropertyName("establishment")]
    public string? RawEstablishment { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    [JsonPropertyName("cnpj")]
    public string? RawCnpj { get; set; }

    public DateTime? Date { get; set; }

    public decimal? Total { get; set; }

    public List<ParsedItem>? Items { get; set; }

    public string ParserVersion { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    public List<string>? ParseErrors { get; set; }
}
