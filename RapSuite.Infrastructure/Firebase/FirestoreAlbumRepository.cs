using Microsoft.Extensions.Options;
using RapSuite.Domain.Entities;
using RapSuite.Domain.Interfaces;
using RapSuite.Infrastructure.Configuration;

namespace RapSuite.Infrastructure.Firebase;

public class FirestoreAlbumRepository : FirestoreServiceBase, IAlbumRepository
{
    public FirestoreAlbumRepository(HttpClient httpClient, IOptions<FirebaseConfig> config)
        : base(httpClient, config) { }

    public async Task<List<Album>> GetByUserAsync(string userId)
    {
        var dict = await GetAsync<Dictionary<string, AlbumRecord>>("albums", userId);
        if (dict == null) return [];

        return dict.Select(kvp => ToAlbum(kvp.Key, kvp.Value))
                   .OrderBy(a => a.CreatedAt)
                   .ToList();
    }

    public async Task<Album?> GetByIdAsync(string userId, string albumId)
    {
        var record = await GetAsync<AlbumRecord>("albums", userId, albumId);
        return record == null ? null : ToAlbum(albumId, record);
    }

    public async Task<Album?> CreateAsync(string userId, Album album)
    {
        var albumId = Guid.NewGuid().ToString("N")[..12];

        var body = new AlbumRecord
        {
            Name = album.Name,
            UserId = userId,
            CoverImageUrl = album.CoverImageUrl ?? "",
            CreatedAt = DateTime.UtcNow.ToString("o"),
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };

        var success = await PutAsync(body, "albums", userId, albumId);
        if (!success) return null;

        album.Id = albumId;
        album.UserId = userId;
        return album;
    }

    public async Task<bool> UpdateAsync(string userId, Album album)
    {
        var body = new Dictionary<string, string>
        {
            ["name"] = album.Name,
            ["updatedAt"] = DateTime.UtcNow.ToString("o")
        };

        return await PatchAsync(body, "albums", userId, album.Id);
    }

    public async Task<bool> DeleteAsync(string userId, string albumId)
    {
        // Delete all songs for this album first
        await DeleteAtAsync("songs", userId, albumId);
        return await DeleteAtAsync("albums", userId, albumId);
    }

    private static Album ToAlbum(string id, AlbumRecord r) => new()
    {
        Id = id,
        Name = r.Name,
        UserId = r.UserId,
        CoverImageUrl = r.CoverImageUrl,
        CreatedAt = DateTime.TryParse(r.CreatedAt, out var c) ? c : DateTime.UtcNow,
        UpdatedAt = DateTime.TryParse(r.UpdatedAt, out var u) ? u : DateTime.UtcNow
    };

    private sealed class AlbumRecord
    {
        public string Name { get; set; } = "";
        public string UserId { get; set; } = "";
        public string CoverImageUrl { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public string UpdatedAt { get; set; } = "";
    }
}
