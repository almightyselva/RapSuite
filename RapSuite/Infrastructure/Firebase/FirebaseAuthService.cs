using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
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

    private string DbUrl => _config.DatabaseUrl;
    private string Key => _config.ApiKey;

    public async Task<(FirebaseAuthResponse? Response, string? Error)> SignUpAsync(string email, string password)
    {
        // Check if email already registered
        var encodedEmail = EncodeEmail(email);
        var lookupUrl = $"{DbUrl}/emailIndex/{encodedEmail}.json?key={Key}";
        var lookupResponse = await _httpClient.GetAsync(lookupUrl);
        if (lookupResponse.IsSuccessStatusCode)
        {
            var existingUid = await lookupResponse.Content.ReadAsStringAsync();
            if (existingUid != "null")
                return (null, "EMAIL_EXISTS");
        }

        var userId = Guid.NewGuid().ToString("N")[..24];
        var passwordHash = HashPassword(password);

        var userData = new
        {
            email,
            passwordHash,
            displayName = "",
            createdAt = DateTime.UtcNow.ToString("o")
        };

        // Store user document
        var userUrl = $"{DbUrl}/users/{userId}.json?key={Key}";
        var userResponse = await _httpClient.PutAsJsonAsync(userUrl, userData);
        if (!userResponse.IsSuccessStatusCode)
        {
            var raw = await userResponse.Content.ReadAsStringAsync();
            return (null, $"Failed to create user: {raw}");
        }

        // Store email-to-uid index for login lookup
        var indexUrl = $"{DbUrl}/emailIndex/{encodedEmail}.json?key={Key}";
        await _httpClient.PutAsJsonAsync(indexUrl, userId);

        return (new FirebaseAuthResponse
        {
            LocalId = userId,
            Email = email,
            IdToken = userId,
            RefreshToken = userId
        }, null);
    }

    public async Task<(FirebaseAuthResponse? Response, string? Error)> SignInAsync(string email, string password)
    {
        // Look up userId by email
        var encodedEmail = EncodeEmail(email);
        var lookupUrl = $"{DbUrl}/emailIndex/{encodedEmail}.json?key={Key}";
        var lookupResponse = await _httpClient.GetAsync(lookupUrl);
        if (!lookupResponse.IsSuccessStatusCode)
            return (null, "EMAIL_NOT_FOUND");

        var uidJson = await lookupResponse.Content.ReadAsStringAsync();
        if (uidJson is "null" or "")
            return (null, "EMAIL_NOT_FOUND");

        var userId = JsonSerializer.Deserialize<string>(uidJson);
        if (string.IsNullOrEmpty(userId))
            return (null, "EMAIL_NOT_FOUND");

        // Get user data
        var userUrl = $"{DbUrl}/users/{userId}.json?key={Key}";
        var userResponse = await _httpClient.GetAsync(userUrl);
        if (!userResponse.IsSuccessStatusCode)
            return (null, "EMAIL_NOT_FOUND");

        var userJson = await userResponse.Content.ReadAsStringAsync();
        if (userJson is "null" or "")
            return (null, "EMAIL_NOT_FOUND");

        var user = JsonSerializer.Deserialize<UserRecord>(userJson, JsonOptions);
        if (user == null)
            return (null, "EMAIL_NOT_FOUND");

        // Verify password
        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash)
            return (null, "INVALID_PASSWORD");

        return (new FirebaseAuthResponse
        {
            LocalId = userId,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IdToken = userId,
            RefreshToken = userId
        }, null);
    }

    public async Task<bool> UpdateProfileAsync(string idToken, string displayName)
    {
        var url = $"{DbUrl}/users/{idToken}/displayName.json?key={Key}";
        var response = await _httpClient.PutAsJsonAsync(url, displayName);
        return response.IsSuccessStatusCode;
    }

    private static string EncodeEmail(string email) =>
        email.Replace(".", ",");

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private sealed class UserRecord
    {
        public string Email { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string PasswordHash { get; set; } = "";
    }
}
