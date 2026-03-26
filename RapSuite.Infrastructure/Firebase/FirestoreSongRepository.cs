using Microsoft.Extensions.Options;
using RapSuite.Domain.Entities;
using RapSuite.Domain.Interfaces;
using RapSuite.Infrastructure.Configuration;

namespace RapSuite.Infrastructure.Firebase;

public class FirestoreSongRepository : FirestoreServiceBase, ISongRepository
{
    public FirestoreSongRepository(HttpClient httpClient, IOptions<FirebaseConfig> config)
        : base(httpClient, config) { }

    public async Task<List<Song>> GetByAlbumAsync(string userId, string albumId)
    {
        var dict = await GetAsync<Dictionary<string, SongRecord>>("songs", userId, albumId);
        if (dict == null) return [];

        return dict.Select(kvp => ToSong(kvp.Key, kvp.Value))
                   .OrderBy(s => s.CreatedAt)
                   .ToList();
    }

    public async Task<Song?> GetByIdAsync(string userId, string albumId, string songId)
    {
        var record = await GetAsync<SongRecord>("songs", userId, albumId, songId);
        return record == null ? null : ToSong(songId, record);
    }

    public async Task<Song?> CreateAsync(string userId, string albumId, Song song)
    {
        var songId = Guid.NewGuid().ToString("N")[..12];

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

        var success = await PutAsync(body, "songs", userId, albumId, songId);
        if (!success) return null;

        song.Id = songId;
        song.AlbumId = albumId;
        song.UserId = userId;
        return song;
    }

    public async Task<bool> UpdateAsync(string userId, string albumId, Song song)
    {
        var body = new Dictionary<string, object>
        {
            ["title"] = song.Title,
            ["lyrics"] = song.Lyrics,
            ["wordCount"] = song.WordCount,
            ["estimatedDuration"] = song.EstimatedDuration,
            ["updatedAt"] = DateTime.UtcNow.ToString("o")
        };

        return await PatchAsync(body, "songs", userId, albumId, song.Id);
    }

    public async Task<bool> DeleteAsync(string userId, string albumId, string songId)
    {
        return await DeleteAtAsync("songs", userId, albumId, songId);
    }

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
