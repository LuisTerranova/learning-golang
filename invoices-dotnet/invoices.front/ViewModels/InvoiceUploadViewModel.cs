using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using invoices.core.Models;
using invoices.core.Services.Abstractions;

namespace invoices.front.ViewModels;

public partial class InvoiceUploadViewModel(IInvoiceService _invoiceService) : ViewModelBase
{
    public IStorageProvider? StorageProvider { get; set; }

    [ObservableProperty]
    private ObservableCollection<PendingFile> _pendingFiles = new();

    [ObservableProperty]
    private bool _isUploading;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private int _uploadedCount;

    public int TotalFiles => PendingFiles.Count;
    public bool HasFiles => PendingFiles.Count > 0;
    public string UploadButtonText => TotalFiles == 0 ? "Select PDFs"
        : IsUploading
            ? $"Uploading {UploadedCount}/{TotalFiles}..."
            : $"Process All ({TotalFiles})";

    partial void OnPendingFilesChanged(ObservableCollection<PendingFile> value)
    {
        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(TotalFiles));
        OnPropertyChanged(nameof(UploadButtonText));
    }

    partial void OnUploadedCountChanged(int value) => OnPropertyChanged(nameof(UploadButtonText));

    partial void OnIsUploadingChanged(bool value) => OnPropertyChanged(nameof(UploadButtonText));

    [RelayCommand]
    private async Task SelectFile()
    {
        if (StorageProvider is null) return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("PDF Files")
                {
                    Patterns = new[] { "*.pdf" },
                    MimeTypes = new[] { "application/pdf" }
                }
            }
        });

        if (files.Count == 0) return;

        var loaded = new List<PendingFile>();

        foreach (var file in files)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);

                loaded.Add(new PendingFile(file.Name, ms.ToArray()));
            }
            catch (Exception ex)
            {
                loaded.Add(new PendingFile(file.Name, null, ex.Message));
            }
        }

        PendingFiles = new ObservableCollection<PendingFile>(loaded);
        UploadedCount = 0;
        StatusMessage = loaded.Any(f => f.Error is not null)
            ? $"Loaded {loaded.Count(f => f.Error is null)}/{loaded.Count} file(s). Some failed."
            : $"Loaded {loaded.Count} file(s). Ready.";
    }

    [RelayCommand]
    private void RemoveFile(int index)
    {
        if (index < 0 || index >= PendingFiles.Count) return;
        PendingFiles.RemoveAt(index);
    }

    [RelayCommand]
    private void ClearFiles()
    {
        PendingFiles.Clear();
        UploadedCount = 0;
        StatusMessage = null;
    }

    [RelayCommand]
    private async Task Upload()
    {
        var validFiles = PendingFiles.Where(f => f.Data is not null).ToList();
        if (validFiles.Count == 0)
        {
            StatusMessage = "No valid files to upload.";
            return;
        }

        IsUploading = true;
        UploadedCount = 0;

        var total = validFiles.Count;
        var failures = 0;

        foreach (var file in validFiles)
        {
            StatusMessage = $"Sending {UploadedCount + 1}/{total}: {file.FileName}...";

            try
            {
                var raw = new RawInvoice
                {
                    FileName = file.FileName,
                    ImageData = file.Data!
                };

                await _invoiceService.SendInvoicesToProcessAsync(raw, CancellationToken.None);
            }
            catch (Exception ex)
            {
                failures++;
                PendingFiles.First(f => f.FileName == file.FileName).Error = ex.Message;
            }

            UploadedCount++;
        }

        if (failures == 0)
            StatusMessage = $"All {total} invoice(s) sent for OCR processing.";
        else
            StatusMessage = $"{total - failures}/{total} sent. {failures} failed.";

        IsUploading = false;
    }
}

public partial class PendingFile(string fileName, byte[]? data, string? error = null) : ObservableObject
{
    public string FileName { get; } = fileName;
    public byte[]? Data { get; } = data;

    [ObservableProperty]
    private string? _error = error;
}
