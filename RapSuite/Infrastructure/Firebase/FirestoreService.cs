using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RapSuite.Configuration;
using RapSuite.Domain.Music;

namespace RapSuite.Infrastructure.Firebase;

public class FirestoreService : IFirestoreService
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseConfig _config;

    public FirestoreService(HttpClient httpClient, IOptions<FirebaseConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    private string BaseUrl => _config.FirestoreBaseUrl;

    private void SetAuth(string idToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
    }

    // ────────────────────────────── Albums ──────────────────────────────

    public async Task<List<Album>> GetAlbumsAsync(string userId, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums?orderBy=createdAt";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new List<Album>();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var albums = new List<Album>();
        if (doc.RootElement.TryGetProperty("documents", out var documents))
        {
            foreach (var docElement in documents.EnumerateArray())
            {
                albums.Add(ParseAlbum(docElement));
            }
        }
        return albums;
    }

    public async Task<Album?> GetAlbumAsync(string userId, string albumId, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return ParseAlbum(doc.RootElement);
    }

    public async Task<Album?> CreateAlbumAsync(string userId, Album album, string idToken)
    {
        SetAuth(idToken);
        var albumId = Guid.NewGuid().ToString("N")[..12];
        var url = $"{BaseUrl}/users/{userId}/albums?documentId={albumId}";

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["name"] = new { stringValue = album.Name },
                ["userId"] = new { stringValue = userId },
                ["coverImageUrl"] = new { stringValue = album.CoverImageUrl ?? "" },
                ["createdAt"] = new { timestampValue = DateTime.UtcNow.ToString("o") },
                ["updatedAt"] = new { timestampValue = DateTime.UtcNow.ToString("o") }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode) return null;

        album.Id = albumId;
        album.UserId = userId;
        return album;
    }

    public async Task<bool> UpdateAlbumAsync(string userId, Album album, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums/{album.Id}?updateMask.fieldPaths=name&updateMask.fieldPaths=updatedAt";

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["name"] = new { stringValue = album.Name },
                ["updatedAt"] = new { timestampValue = DateTime.UtcNow.ToString("o") }
            }
        };

        var response = await _httpClient.PatchAsJsonAsync(url, body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAlbumAsync(string userId, string albumId, string idToken)
    {
        SetAuth(idToken);
        var songs = await GetSongsAsync(userId, albumId, idToken);
        foreach (var song in songs)
        {
            await DeleteSongAsync(userId, albumId, song.Id, idToken);
        }

        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}";
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    // ────────────────────────────── Songs ──────────────────────────────

    public async Task<List<Song>> GetSongsAsync(string userId, string albumId, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}/songs";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return new List<Song>();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var songs = new List<Song>();
        if (doc.RootElement.TryGetProperty("documents", out var documents))
        {
            foreach (var docElement in documents.EnumerateArray())
            {
                songs.Add(ParseSong(docElement));
            }
        }
        return songs;
    }

    public async Task<Song?> GetSongAsync(string userId, string albumId, string songId, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}/songs/{songId}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return ParseSong(doc.RootElement);
    }

    public async Task<Song?> CreateSongAsync(string userId, string albumId, Song song, string idToken)
    {
        SetAuth(idToken);
        var songId = Guid.NewGuid().ToString("N")[..12];
        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}/songs?documentId={songId}";

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["title"] = new { stringValue = song.Title },
                ["lyrics"] = new { stringValue = song.Lyrics },
                ["language"] = new { stringValue = song.Language },
                ["mood"] = new { stringValue = song.Mood },
                ["generationType"] = new { stringValue = song.GenerationType },
                ["wordCount"] = new { integerValue = song.WordCount.ToString() },
                ["estimatedDuration"] = new { stringValue = song.EstimatedDuration },
                ["albumId"] = new { stringValue = albumId },
                ["userId"] = new { stringValue = userId },
                ["createdAt"] = new { timestampValue = DateTime.UtcNow.ToString("o") },
                ["updatedAt"] = new { timestampValue = DateTime.UtcNow.ToString("o") }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode) return null;

        song.Id = songId;
        song.AlbumId = albumId;
        song.UserId = userId;
        return song;
    }

    public async Task<bool> UpdateSongAsync(string userId, string albumId, Song song, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}/songs/{song.Id}?updateMask.fieldPaths=title&updateMask.fieldPaths=lyrics&updateMask.fieldPaths=wordCount&updateMask.fieldPaths=estimatedDuration&updateMask.fieldPaths=updatedAt";

        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["title"] = new { stringValue = song.Title },
                ["lyrics"] = new { stringValue = song.Lyrics },
                ["wordCount"] = new { integerValue = song.WordCount.ToString() },
                ["estimatedDuration"] = new { stringValue = song.EstimatedDuration },
                ["updatedAt"] = new { timestampValue = DateTime.UtcNow.ToString("o") }
            }
        };

        var response = await _httpClient.PatchAsJsonAsync(url, body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSongAsync(string userId, string albumId, string songId, string idToken)
    {
        SetAuth(idToken);
        var url = $"{BaseUrl}/users/{userId}/albums/{albumId}/songs/{songId}";
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    // ────────────────────────────── User Profile ──────────────────────────────

    public async Task SaveUserProfileAsync(string userId, AppUser user, string idToken)
    {
        SetAuth(idToken);
        var body = new
        {
            fields = new Dictionary<string, object>
            {
                ["displayName"] = new { stringValue = user.DisplayName },
                ["email"] = new { stringValue = user.Email },
                ["createdAt"] = new { timestampValue = user.CreatedAt.ToString("o") }
            }
        };

        var patchUrl = $"{BaseUrl}/users/{userId}";
        await _httpClient.PatchAsJsonAsync(patchUrl, body);
    }

    // ────────────────────────────── Parsers ──────────────────────────────

    private static Album ParseAlbum(JsonElement element)
    {
        var fields = element.GetProperty("fields");
        var name = element.GetProperty("name").GetString() ?? "";
        var id = name.Split('/').Last();

        return new Album
        {
            Id = id,
            Name = GetStringField(fields, "name"),
            UserId = GetStringField(fields, "userId"),
            CoverImageUrl = GetStringField(fields, "coverImageUrl"),
            CreatedAt = GetTimestampField(fields, "createdAt"),
            UpdatedAt = GetTimestampField(fields, "updatedAt")
        };
    }

    private static Song ParseSong(JsonElement element)
    {
        var fields = element.GetProperty("fields");
        var name = element.GetProperty("name").GetString() ?? "";
        var id = name.Split('/').Last();

        return new Song
        {
            Id = id,
            Title = GetStringField(fields, "title"),
            Lyrics = GetStringField(fields, "lyrics"),
            Language = GetStringField(fields, "language"),
            Mood = GetStringField(fields, "mood"),
            GenerationType = GetStringField(fields, "generationType"),
            WordCount = GetIntField(fields, "wordCount"),
            EstimatedDuration = GetStringField(fields, "estimatedDuration"),
            AlbumId = GetStringField(fields, "albumId"),
            UserId = GetStringField(fields, "userId"),
            CreatedAt = GetTimestampField(fields, "createdAt"),
            UpdatedAt = GetTimestampField(fields, "updatedAt")
        };
    }

    private static string GetStringField(JsonElement fields, string fieldName)
    {
        if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("stringValue", out var val))
            return val.GetString() ?? "";
        return "";
    }

    private static int GetIntField(JsonElement fields, string fieldName)
    {
        if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("integerValue", out var val))
            return int.TryParse(val.GetString(), out var result) ? result : 0;
        return 0;
    }

    private static DateTime GetTimestampField(JsonElement fields, string fieldName)
    {
        if (fields.TryGetProperty(fieldName, out var field) && field.TryGetProperty("timestampValue", out var val))
            return DateTime.TryParse(val.GetString(), out var result) ? result : DateTime.UtcNow;
        return DateTime.UtcNow;
    }
}
