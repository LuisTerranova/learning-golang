using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.front.ViewModels;

public partial class InvoiceDetailViewModel(
    IInvoiceService _invoiceService,
    HttpClient _httpClient) : ViewModelBase
{
    [ObservableProperty]
    private Invoice? _invoice;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string? _editEstablishment;

    [ObservableProperty]
    private string? _editCnpj;

    [ObservableProperty]
    private DateTimeOffset? _editDate;

    [ObservableProperty]
    private decimal? _editTotal;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isOpeningOriginal;

    public bool HasErrors => Invoice?.ParseErrors.Count > 0;

    public bool HasRawText => !string.IsNullOrWhiteSpace(Invoice?.RawText);

    public Guid InvoiceId { get; set; }

    partial void OnInvoiceChanged(Invoice? value)
    {
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(HasRawText));
    }

    [RelayCommand]
    private async Task LoadInvoice()
    {
        IsLoading = true;
        try
        {
            Invoice = await _invoiceService.GetByIdAsync(InvoiceId, CancellationToken.None);
            if (Invoice is not null)
            {
                EditEstablishment = Invoice.Establishment;
                EditCnpj = Invoice.Cnpj;
                EditDate = Invoice.Date;
                EditTotal = Invoice.Total;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ViewOriginal()
    {
        if (IsOpeningOriginal) return;

        IsOpeningOriginal = true;
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync($"api/invoices/{InvoiceId}/raw-image");
            var ext = Path.GetExtension(Invoice?.RawInvoice?.FileName) ?? ".pdf";
            var tempPath = Path.Combine(Path.GetTempPath(), $"invoice-{InvoiceId}{ext}");
            await File.WriteAllBytesAsync(tempPath, bytes);

            using var process = new Process();
            process.StartInfo.FileName = tempPath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open original: {ex.Message}");
        }
        finally
        {
            IsOpeningOriginal = false;
        }
    }

    [RelayCommand]
    private void Edit()
    {
        if (Invoice is null) return;
        EditEstablishment = Invoice.Establishment;
        EditCnpj = Invoice.Cnpj;
        EditDate = Invoice.Date;
        EditTotal = Invoice.Total;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Invoice is null) return;

        Invoice.Establishment = EditEstablishment;
        Invoice.Cnpj = EditCnpj;
        Invoice.Date = EditDate;
        Invoice.Total = EditTotal;

        await _invoiceService.UpdateAsync(Invoice, CancellationToken.None);
        IsEditing = false;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (Invoice is null) return;
        EditEstablishment = Invoice.Establishment;
        EditCnpj = Invoice.Cnpj;
        EditDate = Invoice.Date;
        EditTotal = Invoice.Total;
        IsEditing = false;
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (Invoice is null) return;
        await _invoiceService.DeleteAsync(Invoice.Id, CancellationToken.None);
    }
}
