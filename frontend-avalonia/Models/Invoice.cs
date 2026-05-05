using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace frontend_avalonia.Models;

public class Invoice
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("raw_id")]
    public Guid RawId { get; set; }

    [JsonPropertyName("raw_text")]
    public string? RawText { get; set; }

    [JsonPropertyName("access_key")]
    public string? AccessKey { get; set; }

    [JsonPropertyName("establishment")]
    public string? Establishment { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }

    [JsonPropertyName("items")]
    public List<ParsedItem> Items { get; set; } = new();

    [JsonPropertyName("parser_version")]
    public string ParserVersion { get; set; } = string.Empty;

    [JsonPropertyName("is_valid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("parse_errors")]
    public List<string> ParseErrors { get; set; } = new();
}