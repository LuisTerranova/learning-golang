namespace invoices.core.Models.Dto;

public class ProcessInvoiceRequest
{
    public string FileName { get; set; } = string.Empty;
    public byte[] ImageData { get; set; } = [];
}
