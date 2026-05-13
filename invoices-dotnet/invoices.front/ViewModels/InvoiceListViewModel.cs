using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using invoices.core.Models;
using invoices.core.Services.Abstractions;
using invoices.front.Services.Abstractions;

namespace invoices.front.ViewModels;

public partial class InvoiceListViewModel(
    IInvoiceService _invoiceService,
    IInvoiceExportService _exportService) : ViewModelBase
{
    public IStorageProvider? StorageProvider { get; set; }

    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    private ObservableCollection<Invoice> _selectedInvoices = new();

    [ObservableProperty]
    private ObservableCollection<YearMonthGroup> _groups = new();

    [ObservableProperty]
    private YearMonthGroup? _selectedGroup;

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

    [ObservableProperty]
    private int? _activeYear;

    [ObservableProperty]
    private int? _activeMonth;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string? _exportStatus;

    [ObservableProperty]
    private string? _errorMessage;

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));

    public bool CanGoToPrevious => CurrentPage > 1;
    public bool CanGoToNext => CurrentPage < TotalPages;

    public bool HasSelection => SelectedInvoices.Count > 0;
    public bool CanExport => ActiveYear.HasValue && ActiveMonth.HasValue;
    public bool IsAllInvoicesSelected => !ActiveYear.HasValue;

    public string DeleteButtonText => HasSelection
        ? $"Delete Selected ({SelectedInvoices.Count})"
        : "Delete Selected";

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

    partial void OnSelectedInvoicesChanged(ObservableCollection<Invoice> value)
    {
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(DeleteButtonText));
    }

    partial void OnActiveYearChanged(int? value)
    {
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(IsAllInvoicesSelected));
    }

    partial void OnActiveMonthChanged(int? value)
    {
        OnPropertyChanged(nameof(CanExport));
    }

    partial void OnSelectedGroupChanged(YearMonthGroup? value)
    {
        ActiveYear = value?.Year;
        ActiveMonth = value?.Month;
        CurrentPage = 1;
        _ = LoadInvoicesCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadInvoices()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            TotalCount = await _invoiceService.GetCountAsync(SearchText, ActiveYear, ActiveMonth, CancellationToken.None);
            var invoices = await _invoiceService.GetAllAsync(CurrentPage, PageSize, SearchText, SortBy, Ascending, ActiveYear, ActiveMonth, CancellationToken.None);
            Invoices = new ObservableCollection<Invoice>(invoices);
            await LoadGroups();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load invoices: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadGroups()
    {
        try
        {
            var groups = await _invoiceService.GetGroupsAsync(CancellationToken.None);
            Groups = new ObservableCollection<YearMonthGroup>(groups);
        }
        catch
        {
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

    [RelayCommand]
    private void SelectGroup(YearMonthGroup? group)
    {
        SelectedGroup = group;
    }

    [RelayCommand]
    private async Task DeleteSelected()
    {
        if (SelectedInvoices.Count == 0) return;
        var ids = SelectedInvoices.Select(i => i.Id).ToList();
        await _invoiceService.DeleteManyAsync(ids, CancellationToken.None);
        SelectedInvoices.Clear();
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(DeleteButtonText));
        await LoadInvoices();
    }

    [RelayCommand]
    private async Task DeleteInvoice(Guid id)
    {
        await _invoiceService.DeleteAsync(id, CancellationToken.None);
        await LoadInvoices();
    }

    [RelayCommand]
    private async Task Export()
    {
        if (!ActiveYear.HasValue || !ActiveMonth.HasValue) return;

        IsExporting = true;
        ExportStatus = "Generating report...";

        try
        {
            ExportStatus = await _exportService.ExportMonthlyReportAsync(
                ActiveYear.Value, ActiveMonth.Value, StorageProvider);
        }
        finally
        {
            IsExporting = false;
        }
    }

    public event EventHandler<Guid>? OpenDetailRequested;

    [RelayCommand]
    private void OpenDetail(Guid id)
    {
        OpenDetailRequested?.Invoke(this, id);
    }
}
