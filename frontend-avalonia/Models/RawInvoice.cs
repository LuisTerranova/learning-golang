using System;
using System.Text.Json.Serialization;

namespace frontend_avalonia.Models;

public class RawInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonIgnore]
    public string FileName { get; set; }
    public byte[] ImageData { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
