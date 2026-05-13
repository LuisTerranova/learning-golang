using Avalonia;
using Avalonia.Controls;
using invoices.front.ViewModels;

namespace invoices.front.Views;

public partial class InvoiceUploadView : UserControl
{
    public InvoiceUploadView()
    {
        InitializeComponent();

        // DataContext arrives AFTER OnAttachedToVisualTree when the view is
        // inflated from a ContentControl DataTemplate, so we must handle both.
        DataContextChanged += (_, _) => WireStorageProvider();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        WireStorageProvider();
    }

    private void WireStorageProvider()
    {
        if (DataContext is InvoiceUploadViewModel vm)
            vm.StorageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
    }
}
