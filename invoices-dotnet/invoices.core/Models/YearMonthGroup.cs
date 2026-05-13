namespace invoices.core.Models;

public class YearMonthGroup
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }

    public string FullLabel => $"{Year:D4}-{Month:D2} ({Count})";
}
