using System;

namespace invoices.core.Models;

public class Establishment
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Cnpj { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
