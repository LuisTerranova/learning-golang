namespace invoices.core.Models;

public class BatchDeleteRequest
{
    public List<Guid> Ids { get; set; } = new();
}
