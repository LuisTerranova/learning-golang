namespace invoices.core.Models;

public class ParsedItem
{
    public string? Name { get; set; }

    public int? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal? Total { get; set; }
}
