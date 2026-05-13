using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.front.ViewModels;

public partial class InvoiceListViewModel(IInvoiceService _invoiceService) : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _sortBy;

    [ObservableProperty]
    private bool _ascending;

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public bool CanGoToPrevious => CurrentPage > 1;
    public bool CanGoToNext => CurrentPage < TotalPages;

    partial void OnCurrentPageChanged(int value)
    {
        OnPropertyChanged(nameof(CanGoToPrevious));
        OnPropertyChanged(nameof(CanGoToNext));
    }

    partial void OnTotalCountChanged(int value)
    {
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(CanGoToNext));
    }

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task LoadInvoices()
    {
        Console.WriteLine("[LoadInvoices] START");
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            TotalCount = await _invoiceService.GetCountAsync(SearchText, CancellationToken.None);
            Console.WriteLine($"[LoadInvoices] TotalCount = {TotalCount}");

            var invoices = await _invoiceService.GetAllAsync(CurrentPage, PageSize, SearchText, SortBy, Ascending, CancellationToken.None);
            Console.WriteLine($"[LoadInvoices] Fetched {invoices.Count} invoices");

            foreach (var inv in invoices)
                Console.WriteLine($"  -> {inv.Id} | {inv.Establishment} | {inv.Date} | {inv.Total}");

            Invoices = new ObservableCollection<Invoice>(invoices);
            Console.WriteLine($"[LoadInvoices] ObservableCollection set with {Invoices.Count} items");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load invoices: {ex.Message}";
            Console.WriteLine($"[LoadInvoices] ERROR: {ex}");
        }
        finally
        {
            IsLoading = false;
            Console.WriteLine("[LoadInvoices] END");
        }
    }

    [RelayCommand]
    private async Task Search()
    {
        CurrentPage = 1;
        await LoadInvoices();
    }

    [RelayCommand]
    private async Task GoToNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadInvoices();
        }
    }

    [RelayCommand]
    private async Task GoToPreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadInvoices();
        }
    }

    [RelayCommand]
    private async Task Sort(string column)
    {
        if (SortBy == column)
            Ascending = !Ascending;
        else
        {
            SortBy = column;
            Ascending = true;
        }
        await LoadInvoices();
    }

    public event EventHandler<Guid>? OpenDetailRequested;

    [RelayCommand]
    private void OpenDetail(Guid id)
    {
        OpenDetailRequested?.Invoke(this, id);
    }

    [RelayCommand]
    private async Task DeleteInvoice(Guid id)
    {
        await _invoiceService.DeleteAsync(id, CancellationToken.None);
        await LoadInvoices();
    }
}
