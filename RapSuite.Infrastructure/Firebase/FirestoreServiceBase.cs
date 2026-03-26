using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RapSuite.Infrastructure.Configuration;

namespace RapSuite.Infrastructure.Firebase;

public abstract class FirestoreServiceBase
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseConfig _config;
    protected static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected FirestoreServiceBase(HttpClient httpClient, IOptions<FirebaseConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    protected string BuildUrl(params string[] segments)
    {
        var path = string.Join("/", segments);
        return $"{_config.DatabaseUrl}/{path}.json?key={_config.ApiKey}";
    }

    protected async Task<T?> GetAsync<T>(params string[] segments)
    {
        var url = BuildUrl(segments);
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return default;

        var json = await response.Content.ReadAsStringAsync();
        if (json is "null" or "") return default;

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    protected async Task<string?> GetRawAsync(params string[] segments)
    {
        var url = BuildUrl(segments);
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return json is "null" or "" ? null : json;
    }

    protected async Task<bool> PutAsync<T>(T body, params string[] segments)
    {
        var url = BuildUrl(segments);
        var response = await _httpClient.PutAsJsonAsync(url, body);
        return response.IsSuccessStatusCode;
    }

    protected async Task<bool> PatchAsync<T>(T body, params string[] segments)
    {
        var url = BuildUrl(segments);
        var response = await _httpClient.PatchAsJsonAsync(url, body);
        return response.IsSuccessStatusCode;
    }

    protected async Task<bool> DeleteAtAsync(params string[] segments)
    {
        var url = BuildUrl(segments);
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }
}
