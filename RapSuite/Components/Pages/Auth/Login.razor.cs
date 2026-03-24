using Microsoft.AspNetCore.Components;
using RapSuite.Infrastructure.Firebase;
using RapSuite.Infrastructure.Session;

namespace RapSuite.Components.Pages.Auth;

public partial class Login
{
    [Inject] private IFirebaseAuthService AuthService { get; set; } = default!;
    [Inject] private UserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private LoginModel _loginModel = new();
    private string? _errorMessage;
    private bool _isLoading;

    private class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    protected override void OnInitialized()
    {
        if (Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/");
        }
    }

    private async Task HandleLogin()
    {
        _errorMessage = null;
        _isLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(_loginModel.Email) || string.IsNullOrWhiteSpace(_loginModel.Password))
            {
                _errorMessage = "Please fill in all fields.";
                return;
            }

            var (response, error) = await AuthService.SignInAsync(_loginModel.Email, _loginModel.Password);

            if (response != null)
            {
                Session.SetUser(response);
                Navigation.NavigateTo("/");
            }
            else
            {
                _errorMessage = FormatError(error);
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static string FormatError(string? error) => error switch
    {
        "EMAIL_NOT_FOUND" => "No account found with this email.",
        "INVALID_PASSWORD" => "Incorrect password.",
        "USER_DISABLED" => "This account has been disabled.",
        "INVALID_LOGIN_CREDENTIALS" => "Invalid email or password.",
        _ => error ?? "Sign in failed. Please try again."
    };
}
