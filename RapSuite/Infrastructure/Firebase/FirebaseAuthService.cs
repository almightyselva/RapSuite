using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RapSuite.Configuration;
using RapSuite.Domain.Auth;

namespace RapSuite.Infrastructure.Firebase;

public class FirebaseAuthService : IFirebaseAuthService
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseConfig _config;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FirebaseAuthService(HttpClient httpClient, IOptions<FirebaseConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<(FirebaseAuthResponse? Response, string? Error)> SignUpAsync(string email, string password)
    {
        var request = new FirebaseSignUpRequest { Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync(
            $"{_config.AuthBaseUrl}/accounts:signUp?key={_config.ApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>(JsonOptions);
            return (result, null);
        }

        var error = await response.Content.ReadFromJsonAsync<FirebaseAuthError>(JsonOptions);
        return (null, error?.Error?.Message ?? "Sign up failed");
    }

    public async Task<(FirebaseAuthResponse? Response, string? Error)> SignInAsync(string email, string password)
    {
        var request = new FirebaseSignInRequest { Email = email, Password = password };
        var response = await _httpClient.PostAsJsonAsync(
            $"{_config.AuthBaseUrl}/accounts:signInWithPassword?key={_config.ApiKey}", request);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<FirebaseAuthResponse>(JsonOptions);
            return (result, null);
        }

        var error = await response.Content.ReadFromJsonAsync<FirebaseAuthError>(JsonOptions);
        return (null, error?.Error?.Message ?? "Sign in failed");
    }

    public async Task<bool> UpdateProfileAsync(string idToken, string displayName)
    {
        var request = new FirebaseUpdateProfileRequest { IdToken = idToken, DisplayName = displayName };
        var response = await _httpClient.PostAsJsonAsync(
            $"{_config.AuthBaseUrl}/accounts:update?key={_config.ApiKey}", request);

        return response.IsSuccessStatusCode;
    }
}
