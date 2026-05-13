namespace invoices.core.Models.Dto;

public class UpdateInvoiceRequest
{
    public string? Establishment { get; set; }
    public string? Cnpj { get; set; }
    public DateTime? Date { get; set; }
    public decimal? Total { get; set; }
    public List<ParsedItemDto> Items { get; set; } = new();
}
