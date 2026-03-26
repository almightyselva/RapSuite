using RapSuite.Domain.Entities;

namespace RapSuite.Domain.Interfaces;

public interface ISongRepository
{
    Task<List<Song>> GetByAlbumAsync(string userId, string albumId);
    Task<Song?> GetByIdAsync(string userId, string albumId, string songId);
    Task<Song?> CreateAsync(string userId, string albumId, Song song);
    Task<bool> UpdateAsync(string userId, string albumId, Song song);
    Task<bool> DeleteAsync(string userId, string albumId, string songId);
}
