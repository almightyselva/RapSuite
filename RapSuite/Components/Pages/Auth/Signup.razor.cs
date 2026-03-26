using Microsoft.AspNetCore.Components;
using RapSuite.Domain.Interfaces;

namespace RapSuite.Components.Pages.Auth;

public partial class Signup
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private IUserSessionService Session { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private SignupModel _signupModel = new();
    private string? _errorMessage;
    private string? _successMessage;
    private bool _isLoading;

    private class SignupModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    protected override void OnInitialized()
    {
        if (Session.IsAuthenticated)
        {
            Navigation.NavigateTo("/");
        }
    }

    private async Task HandleSignup()
    {
        _errorMessage = null;
        _successMessage = null;
        _isLoading = true;

        try
        {
            if (string.IsNullOrWhiteSpace(_signupModel.DisplayName) ||
                string.IsNullOrWhiteSpace(_signupModel.Email) ||
                string.IsNullOrWhiteSpace(_signupModel.Password))
            {
                _errorMessage = "Please fill in all fields.";
                return;
            }

            if (_signupModel.Password.Length < 6)
            {
                _errorMessage = "Password must be at least 6 characters.";
                return;
            }

            if (_signupModel.Password != _signupModel.ConfirmPassword)
            {
                _errorMessage = "Passwords do not match.";
                return;
            }

            var result = await AuthService.SignUpAsync(_signupModel.Email, _signupModel.Password);

            if (result.IsSuccess)
            {
                await AuthService.UpdateProfileAsync(result.Value!.IdToken, _signupModel.DisplayName);
                result.Value!.DisplayName = _signupModel.DisplayName;

                Session.SetUser(result.Value!);
                Navigation.NavigateTo("/");
            }
            else
            {
                _errorMessage = FormatError(result.Error);
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static string FormatError(string? error) => error switch
    {
        "EMAIL_EXISTS" => "An account with this email already exists.",
        "WEAK_PASSWORD : Password should be at least 6 characters" => "Password must be at least 6 characters.",
        "INVALID_EMAIL" => "Please enter a valid email address.",
        _ => error ?? "Sign up failed. Please try again."
    };
}
