using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace invoices.front.Services.Abstractions;

public interface IInvoiceExportService
{
    Task<string?> ExportMonthlyReportAsync(int year, int month, IStorageProvider storageProvider);
}
