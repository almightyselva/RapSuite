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
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FirestoreService(HttpClient httpClient, IOptions<FirebaseConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    private string DbUrl => _config.DatabaseUrl;
    private string Key => _config.ApiKey;

    // ────────────────────────────── Albums ──────────────────────────────

    public async Task<List<Album>> GetAlbumsAsync(string userId, string idToken)
    {
        var url = $"{DbUrl}/albums/{userId}.json?key={Key}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        if (json is "null" or "") return [];

        var dict = JsonSerializer.Deserialize<Dictionary<string, AlbumRecord>>(json, JsonOptions);
        if (dict == null) return [];

        return dict.Select(kvp => ToAlbum(kvp.Key, kvp.Value))
                   .OrderBy(a => a.CreatedAt)
                   .ToList();
    }

    public async Task<Album?> GetAlbumAsync(string userId, string albumId, string idToken)
    {
        var url = $"{DbUrl}/albums/{userId}/{albumId}.json?key={Key}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        if (json is "null" or "") return null;

        var record = JsonSerializer.Deserialize<AlbumRecord>(json, JsonOptions);
        return record == null ? null : ToAlbum(albumId, record);
    }

    public async Task<Album?> CreateAlbumAsync(string userId, Album album, string idToken)
    {
        var albumId = Guid.NewGuid().ToString("N")[..12];
        var url = $"{DbUrl}/albums/{userId}/{albumId}.json?key={Key}";

        var body = new AlbumRecord
        {
            Name = album.Name,
            UserId = userId,
            CoverImageUrl = album.CoverImageUrl ?? "",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };

        var response = await _httpClient.PutAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode) return null;

        album.Id = albumId;
        album.UserId = userId;
        return album;
    }

    public async Task<bool> UpdateAlbumAsync(string userId, Album album, string idToken)
    {
        var url = $"{DbUrl}/albums/{userId}/{album.Id}.json?key={Key}";

        var body = new Dictionary<string, string>
        {
            ["name"] = album.Name,
            ["updatedAt"] = DateTime.UtcNow.ToString("o")
        };

        var response = await _httpClient.PatchAsJsonAsync(url, body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAlbumAsync(string userId, string albumId, string idToken)
    {
        // Delete all songs for this album
        var songsUrl = $"{DbUrl}/songs/{userId}/{albumId}.json?key={Key}";
        await _httpClient.DeleteAsync(songsUrl);

        // Delete the album
        var url = $"{DbUrl}/albums/{userId}/{albumId}.json?key={Key}";
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    // ────────────────────────────── Songs ──────────────────────────────

    public async Task<List<Song>> GetSongsAsync(string userId, string albumId, string idToken)
    {
        var url = $"{DbUrl}/songs/{userId}/{albumId}.json?key={Key}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return [];

        var json = await response.Content.ReadAsStringAsync();
        if (json is "null" or "") return [];

        var dict = JsonSerializer.Deserialize<Dictionary<string, SongRecord>>(json, JsonOptions);
        if (dict == null) return [];

        return dict.Select(kvp => ToSong(kvp.Key, kvp.Value))
                   .OrderBy(s => s.CreatedAt)
                   .ToList();
    }

    public async Task<Song?> GetSongAsync(string userId, string albumId, string songId, string idToken)
    {
        var url = $"{DbUrl}/songs/{userId}/{albumId}/{songId}.json?key={Key}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        if (json is "null" or "") return null;

        var record = JsonSerializer.Deserialize<SongRecord>(json, JsonOptions);
        return record == null ? null : ToSong(songId, record);
    }

    public async Task<Song?> CreateSongAsync(string userId, string albumId, Song song, string idToken)
    {
        var songId = Guid.NewGuid().ToString("N")[..12];
        var url = $"{DbUrl}/songs/{userId}/{albumId}/{songId}.json?key={Key}";

        var body = new SongRecord
        {
            Title = song.Title,
            Lyrics = song.Lyrics,
            Language = song.Language,
            Mood = song.Mood,
            GenerationType = song.GenerationType,
            WordCount = song.WordCount,
            EstimatedDuration = song.EstimatedDuration,
            AlbumId = albumId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };

        var response = await _httpClient.PutAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode) return null;

        song.Id = songId;
        song.AlbumId = albumId;
        song.UserId = userId;
        return song;
    }

    public async Task<bool> UpdateSongAsync(string userId, string albumId, Song song, string idToken)
    {
        var url = $"{DbUrl}/songs/{userId}/{albumId}/{song.Id}.json?key={Key}";

        var body = new Dictionary<string, object>
        {
            ["title"] = song.Title,
            ["lyrics"] = song.Lyrics,
            ["wordCount"] = song.WordCount,
            ["estimatedDuration"] = song.EstimatedDuration,
            ["updatedAt"] = DateTime.UtcNow.ToString("o")
        };

        var response = await _httpClient.PatchAsJsonAsync(url, body);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSongAsync(string userId, string albumId, string songId, string idToken)
    {
        var url = $"{DbUrl}/songs/{userId}/{albumId}/{songId}.json?key={Key}";
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }

    // ────────────────────────────── User Profile ──────────────────────────────

    public async Task SaveUserProfileAsync(string userId, AppUser user, string idToken)
    {
        var url = $"{DbUrl}/users/{userId}.json?key={Key}";

        var body = new Dictionary<string, string>
        {
            ["displayName"] = user.DisplayName,
            ["email"] = user.Email,
            ["createdAt"] = user.CreatedAt.ToString("o")
        };

        await _httpClient.PatchAsJsonAsync(url, body);
    }

    // ────────────────────────────── Mappers ──────────────────────────────

    private static Album ToAlbum(string id, AlbumRecord r) => new()
    {
        Id = id,
        Name = r.Name,
        UserId = r.UserId,
        CoverImageUrl = r.CoverImageUrl,
        CreatedAt = DateTime.TryParse(r.CreatedAt, out var c) ? c : DateTime.UtcNow,
        UpdatedAt = DateTime.TryParse(r.UpdatedAt, out var u) ? u : DateTime.UtcNow
    };

    private static Song ToSong(string id, SongRecord r) => new()
    {
        Id = id,
        Title = r.Title,
        Lyrics = r.Lyrics,
        Language = r.Language,
        Mood = r.Mood,
        GenerationType = r.GenerationType,
        WordCount = r.WordCount,
        EstimatedDuration = r.EstimatedDuration,
        AlbumId = r.AlbumId,
        UserId = r.UserId,
        CreatedAt = DateTime.TryParse(r.CreatedAt, out var c) ? c : DateTime.UtcNow,
        UpdatedAt = DateTime.TryParse(r.UpdatedAt, out var u) ? u : DateTime.UtcNow
    };

    // ────────────────────────────── DTOs ──────────────────────────────

    private sealed class AlbumRecord
    {
        public string Name { get; set; } = "";
        public string UserId { get; set; } = "";
        public string CoverImageUrl { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public string UpdatedAt { get; set; } = "";
    }

    private sealed class SongRecord
    {
        public string Title { get; set; } = "";
        public string Lyrics { get; set; } = "";
        public string Language { get; set; } = "";
        public string Mood { get; set; } = "";
        public string GenerationType { get; set; } = "";
        public int WordCount { get; set; }
        public string EstimatedDuration { get; set; } = "";
        public string AlbumId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public string UpdatedAt { get; set; } = "";
    }
}
