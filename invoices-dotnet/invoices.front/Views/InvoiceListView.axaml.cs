using Avalonia;
using Avalonia.Controls;
using invoices.core.Models;
using invoices.front.ViewModels;

namespace invoices.front.Views;

public partial class InvoiceListView : UserControl
{
    public InvoiceListView()
    {
        InitializeComponent();
        InvoiceDataGrid.SelectionMode = DataGridSelectionMode.Extended;
        DataContextChanged += (_, _) => WireStorageProvider();
        InvoiceDataGrid.SelectionChanged += (_, _) =>
        {
            if (DataContext is InvoiceListViewModel vm)
            {
                vm.SelectedInvoices.Clear();
                foreach (var item in InvoiceDataGrid.SelectedItems)
                {
                    if (item is Invoice inv)
                        vm.SelectedInvoices.Add(inv);
                }
            }
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        WireStorageProvider();
    }

    private void WireStorageProvider()
    {
        if (DataContext is InvoiceListViewModel vm)
            vm.StorageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
    }
}
