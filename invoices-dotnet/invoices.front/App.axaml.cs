using System;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using invoices.core.Services.Abstractions;
using invoices.front.Services;
using invoices.front.Services.Abstractions;
using invoices.front.Services.Implementations;
using invoices.front.ViewModels;
using invoices.front.Views;

using Microsoft.Extensions.DependencyInjection;

namespace invoices.front;

public partial class App : Application
{
    private ServiceProvider? _provider;
    private Window? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var services = new ServiceCollection();

        services.AddSingleton<AuthService>();
        services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        services.AddSingleton<IAuthClient>(sp => sp.GetRequiredService<AuthService>());
        services.AddSingleton(sp =>
        {
            var handler = sp.GetRequiredService<AuthTokenHandler>();
            handler.InnerHandler = new HttpClientHandler
            {
                // Ignora erro de certificado autoassinado de desenvolvimento local no ambiente de testes/dev
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            return new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7118") };
        });
        services.AddSingleton<AuthTokenHandler>();

        services.AddScoped<IInvoiceService, HttpInvoiceService>();
        services.AddScoped<IEstablishmentService, HttpEstablishmentService>();
        services.AddScoped<IInvoiceExportService, ExcelExportService>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<InvoiceListViewModel>();
        services.AddTransient<InvoiceDetailViewModel>();
        services.AddTransient<InvoiceUploadViewModel>();

        _provider = services.BuildServiceProvider();

        ShowLoginWindow(desktop);

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowLoginWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var loginVm = _provider!.GetRequiredService<LoginViewModel>();
        var loginWindow = new LoginWindow { DataContext = loginVm };

        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        loginVm.LoginSucceeded += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var mainVm = _provider!.GetRequiredService<MainWindowViewModel>();
                var mainWindow = new MainWindow { DataContext = mainVm };

                mainVm.LogoutRequested += (_, _) =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                        mainWindow.Close();
                        _mainWindow = null;
                        ShowLoginWindow(desktop);
                    });
                };

                mainWindow.Show();
                loginWindow.Close();

                _mainWindow = mainWindow;
                desktop.MainWindow = mainWindow;
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            });
        };

        desktop.MainWindow = loginWindow;
        loginWindow.Show();
    }
}
