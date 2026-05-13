using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ClosedXML.Excel;
using invoices.core.Services.Abstractions;
using invoices.front.Services.Abstractions;

namespace invoices.front.Services.Implementations;

public class ExcelExportService(IInvoiceService _invoiceService) : IInvoiceExportService
{
    public async Task<string?> ExportMonthlyReportAsync(int year, int month, IStorageProvider? storageProvider)
    {
        var invoices = await _invoiceService.GetByMonthAsync(year, month, default);
        if (invoices.Count == 0)
            return "No invoices for this period.";

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"{month}-{year}");
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy");

        ws.Cell("A1").Value = $"Monthly Invoice Report - {monthName}";
        ws.Range("A1:G1").Merge().Style.Font.Bold = true;
        ws.Range("A1:G1").Style.Font.FontSize = 16;

        ws.Cell("A3").Value = "Total Invoices:";
        ws.Cell("B3").Value = invoices.Count;
        ws.Cell("D3").Value = "Total Amount:";
        ws.Cell("E3").Value = invoices.Sum(i => i.Total ?? 0);
        ws.Cell("E3").Style.NumberFormat.Format = "R$ #,##0.00";

        ws.Cell("A5").Value = "Establishment";
        ws.Cell("B5").Value = "CNPJ";
        ws.Cell("C5").Value = "Date";
        ws.Cell("D5").Value = "Access Key";
        ws.Cell("E5").Value = "Total";
        ws.Cell("F5").Value = "Items";
        ws.Cell("G5").Value = "Valid";
        ws.Range("A5:G5").Style.Font.Bold = true;

        int row = 6;
        foreach (var inv in invoices)
        {
            ws.Cell(row, 1).Value = inv.Establishment?.Name ?? inv.RawEstablishment ?? "";
            ws.Cell(row, 2).Value = inv.Establishment?.Cnpj ?? inv.RawCnpj ?? "";
            ws.Cell(row, 3).Value = inv.Date?.ToString("dd/MM/yyyy") ?? "";
            ws.Cell(row, 4).Value = inv.AccessKey ?? "";
            ws.Cell(row, 5).Value = (double)(inv.Total ?? 0);
            ws.Cell(row, 5).Style.NumberFormat.Format = "R$ #,##0.00";
            ws.Cell(row, 6).Value = inv.Items?.Count ?? 0;
            ws.Cell(row, 7).Value = inv.IsValid ? "Yes" : "No";
            row++;
        }

        ws.Cell(row, 4).Value = "Total";
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 5).FormulaA1 = $"=SUM(E6:E{row - 1})";
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();

        if (storageProvider == null)
            return "Storage provider unavailable.";

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Invoice Report",
            SuggestedFileName = $"invoice-report-{year}-{month:D2}.xlsx",
            DefaultExtension = "xlsx",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Excel Spreadsheet")
                {
                    Patterns = new[] { "*.xlsx" }
                }
            }
        });

        if (file == null)
            return null;

        await using var stream = await file.OpenWriteAsync();
        workbook.SaveAs(stream);
        return $"Report saved: {file.Name}";
    }
}
