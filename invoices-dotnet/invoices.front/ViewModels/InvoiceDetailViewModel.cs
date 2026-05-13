using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.front.ViewModels;

public partial class InvoiceDetailViewModel(
    IInvoiceService _invoiceService,
    IEstablishmentService _establishmentService,
    HttpClient _httpClient) : ViewModelBase
{
    [ObservableProperty]
    private Invoice? _invoice;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string? _editEstablishment;

    [ObservableProperty]
    private Establishment? _selectedEstablishment;

    [ObservableProperty]
    private ObservableCollection<Establishment> _establishmentSuggestions = new();

    [ObservableProperty]
    private string? _editCnpj;

    [ObservableProperty]
    private DateTime? _editDate;

    [ObservableProperty]
    private decimal? _editTotal;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isOpeningOriginal;

    [ObservableProperty]
    private ObservableCollection<ParsedItem> _editableItems = new();

    [ObservableProperty]
    private ParsedItem? _selectedItem;

    public bool HasErrors => Invoice?.ParseErrors?.Count > 0;

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
                EditEstablishment = Invoice.Establishment?.Name;
                SelectedEstablishment = Invoice.Establishment;
                EditCnpj = Invoice.Establishment?.Cnpj;
                EditDate = Invoice.Date;
                EditTotal = Invoice.Total;
                EditableItems = new ObservableCollection<ParsedItem>(Invoice.Items);
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
        EditEstablishment = Invoice.Establishment?.Name;
        SelectedEstablishment = Invoice.Establishment;
        EditCnpj = Invoice.Establishment?.Cnpj;
        EditDate = Invoice.Date;
        EditTotal = Invoice.Total;
        EditableItems = new ObservableCollection<ParsedItem>(Invoice.Items);
        IsEditing = true;
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Invoice is null) return;

        Invoice.RawEstablishment = EditEstablishment;
        Invoice.RawCnpj = EditCnpj;
        if (SelectedEstablishment != null && SelectedEstablishment.Name == EditEstablishment)
        {
            Invoice.EstablishmentId = SelectedEstablishment.Id;
            Invoice.RawCnpj = SelectedEstablishment.Cnpj ?? EditCnpj;
        }

        Invoice.Date = EditDate;
        Invoice.Total = EditTotal;
        Invoice.Items = EditableItems.ToList();

        await _invoiceService.UpdateAsync(Invoice, CancellationToken.None);

        IsEditing = false;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (Invoice is null) return;
        EditEstablishment = Invoice.Establishment?.Name;
        SelectedEstablishment = Invoice.Establishment;
        EditCnpj = Invoice.Establishment?.Cnpj;
        EditDate = Invoice.Date;
        EditTotal = Invoice.Total;
        EditableItems = new ObservableCollection<ParsedItem>(Invoice.Items);
        IsEditing = false;
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (Invoice is null) return;
        await _invoiceService.DeleteAsync(Invoice.Id, CancellationToken.None);
    }

    [RelayCommand]
    private void AddItem()
    {
        EditableItems.Add(new ParsedItem());
    }

    [RelayCommand]
    private void RemoveItem()
    {
        if (SelectedItem is not null)
            EditableItems.Remove(SelectedItem);
    }

    private CancellationTokenSource? _searchCts;

    partial void OnEditEstablishmentChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        {
            EstablishmentSuggestions.Clear();
            return;
        }

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, _searchCts.Token);
                var results = await _establishmentService.SearchAsync(value, _searchCts.Token);

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    EstablishmentSuggestions.Clear();
                    foreach (var est in results)
                    {
                        EstablishmentSuggestions.Add(est);
                    }
                });
            }
            catch (TaskCanceledException) { }
        });
    }

    partial void OnSelectedEstablishmentChanged(Establishment? value)
    {
        if (value is not null)
        {
            EditCnpj = value.Cnpj;
            EditEstablishment = value.Name;
        }
    }
}
