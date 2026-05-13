using System;
using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using invoices.front.Services;
using invoices.front.Services.Abstractions;
using invoices.front.Services.Implementations;
using invoices.front.ViewModels;
using invoices.front.Views;
using invoices.core.Services.Abstractions;

namespace invoices.front;

public partial class App : Application
{
    private ServiceProvider? _provider;

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
            handler.InnerHandler = new HttpClientHandler();
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5152"),
            };
        });
        services.AddSingleton<AuthTokenHandler>();

        services.AddScoped<IInvoiceService, HttpInvoiceService>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<InvoiceListViewModel>();
        services.AddTransient<InvoiceDetailViewModel>();
        services.AddTransient<InvoiceUploadViewModel>();

        _provider = services.BuildServiceProvider();

        var loginVm = _provider.GetRequiredService<LoginViewModel>();
        var loginWindow = new LoginWindow { DataContext = loginVm };

        // Keep the app alive until we explicitly switch to OnMainWindowClose
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        loginVm.LoginSucceeded += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var mainVm = _provider.GetRequiredService<MainWindowViewModel>();
                var mainWindow = new MainWindow { DataContext = mainVm };

                // Show MainWindow BEFORE closing LoginWindow — if LoginWindow
                // closes first while it's still desktop.MainWindow and
                // ShutdownMode is OnMainWindowClose, the process exits.
                mainWindow.Show();
                loginWindow.Close();

                desktop.MainWindow = mainWindow;
                desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
            });
        };

        desktop.MainWindow = loginWindow;

        base.OnFrameworkInitializationCompleted();
    }
}
