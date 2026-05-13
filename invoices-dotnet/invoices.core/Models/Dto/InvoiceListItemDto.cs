namespace invoices.core.Models.Dto;

public class InvoiceListItemDto
{
    public Guid Id { get; set; }
    public string? AccessKey { get; set; }
    public string? Establishment { get; set; }
    public string? Cnpj { get; set; }
    public DateTime? Date { get; set; }
    public decimal? Total { get; set; }
    public bool IsValid { get; set; }
}
