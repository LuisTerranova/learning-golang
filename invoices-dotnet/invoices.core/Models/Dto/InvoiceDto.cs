namespace invoices.core.Models.Dto;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid RawId { get; set; }
    public string? RawText { get; set; }
    public string? AccessKey { get; set; }
    public string? Establishment { get; set; }
    public string? Cnpj { get; set; }
    public DateTime? Date { get; set; }
    public decimal? Total { get; set; }
    public List<ParsedItemDto> Items { get; set; } = new();
    public string ParserVersion { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<string> ParseErrors { get; set; } = new();
}
