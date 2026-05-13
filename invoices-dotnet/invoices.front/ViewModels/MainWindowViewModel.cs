using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace invoices.front.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;

    [ObservableProperty]
    private ViewModelBase? _activeViewModel;

    public MainWindowViewModel(IServiceProvider services)
    {
        _services = services;
        var initial = _services.GetRequiredService<InvoiceListViewModel>();
        initial.OpenDetailRequested += (_, id) => OpenInvoiceDetail(id);
        _ = initial.LoadInvoicesCommand.ExecuteAsync(null);
        ActiveViewModel = initial;
    }

    [RelayCommand]
    private void NavigateToInvoices()
    {
        var vm = _services.GetRequiredService<InvoiceListViewModel>();
        vm.OpenDetailRequested += (_, id) => OpenInvoiceDetail(id);
        _ = vm.LoadInvoicesCommand.ExecuteAsync(null);
        ActiveViewModel = vm;
    }

    [RelayCommand]
    private void NavigateToUpload()
    {
        ActiveViewModel = _services.GetRequiredService<InvoiceUploadViewModel>();
    }

    [RelayCommand]
    private void OpenInvoiceDetail(Guid id)
    {
        var vm = _services.GetRequiredService<InvoiceDetailViewModel>();
        vm.InvoiceId = id;
        _ = vm.LoadInvoiceCommand.ExecuteAsync(null);
        ActiveViewModel = vm;
    }
}
