using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using invoices.core.Services.Abstractions;

namespace invoices.front.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isLoading;

    public event EventHandler? LoginSucceeded;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password.";
            return;
        }

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var result = await _authService.LoginAsync(Username, Password);

            if (result.Success)
            {
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Login failed.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
