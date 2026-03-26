using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RapSuite.Domain.Common;
using RapSuite.Domain.Interfaces;
using RapSuite.Domain.Models;
using RapSuite.Infrastructure.Configuration;

namespace RapSuite.Infrastructure.Firebase;

public class FirebaseAuthService : IAuthService
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

    public async Task<Result<AuthResponse>> SignUpAsync(string email, string password)
    {
        var encodedEmail = EncodeEmail(email);
        var lookupUrl = $"{DbUrl}/emailIndex/{encodedEmail}.json?key={Key}";
        var lookupResponse = await _httpClient.GetAsync(lookupUrl);
        if (lookupResponse.IsSuccessStatusCode)
        {
            var existingUid = await lookupResponse.Content.ReadAsStringAsync();
            if (existingUid != "null")
                return Result<AuthResponse>.Failure("EMAIL_EXISTS");
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

        var userUrl = $"{DbUrl}/users/{userId}.json?key={Key}";
        var userResponse = await _httpClient.PutAsJsonAsync(userUrl, userData);
        if (!userResponse.IsSuccessStatusCode)
        {
            var raw = await userResponse.Content.ReadAsStringAsync();
            return Result<AuthResponse>.Failure($"Failed to create user: {raw}");
        }

        var indexUrl = $"{DbUrl}/emailIndex/{encodedEmail}.json?key={Key}";
        await _httpClient.PutAsJsonAsync(indexUrl, userId);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            LocalId = userId,
            Email = email,
            IdToken = userId,
            RefreshToken = userId
        });
    }

    public async Task<Result<AuthResponse>> SignInAsync(string email, string password)
    {
        var encodedEmail = EncodeEmail(email);
        var lookupUrl = $"{DbUrl}/emailIndex/{encodedEmail}.json?key={Key}";
        var lookupResponse = await _httpClient.GetAsync(lookupUrl);
        if (!lookupResponse.IsSuccessStatusCode)
            return Result<AuthResponse>.Failure("EMAIL_NOT_FOUND");

        var uidJson = await lookupResponse.Content.ReadAsStringAsync();
        if (uidJson is "null" or "")
            return Result<AuthResponse>.Failure("EMAIL_NOT_FOUND");

        var userId = JsonSerializer.Deserialize<string>(uidJson);
        if (string.IsNullOrEmpty(userId))
            return Result<AuthResponse>.Failure("EMAIL_NOT_FOUND");

        var userUrl = $"{DbUrl}/users/{userId}.json?key={Key}";
        var userResponse = await _httpClient.GetAsync(userUrl);
        if (!userResponse.IsSuccessStatusCode)
            return Result<AuthResponse>.Failure("EMAIL_NOT_FOUND");

        var userJson = await userResponse.Content.ReadAsStringAsync();
        if (userJson is "null" or "")
            return Result<AuthResponse>.Failure("EMAIL_NOT_FOUND");

        var user = JsonSerializer.Deserialize<UserRecord>(userJson, JsonOptions);
        if (user == null)
            return Result<AuthResponse>.Failure("EMAIL_NOT_FOUND");

        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash)
            return Result<AuthResponse>.Failure("INVALID_PASSWORD");

        return Result<AuthResponse>.Success(new AuthResponse
        {
            LocalId = userId,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IdToken = userId,
            RefreshToken = userId
        });
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
