using System.Text.Json.Serialization;

namespace invoices.core.Models;

public class ParsedItem
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }

    [JsonPropertyName("unit_price")]
    public decimal? UnitPrice { get; set; }

    [JsonPropertyName("total")]
    public decimal? Total { get; set; }
}
