using RapSuite.Domain.Entities;

namespace RapSuite.Domain.Interfaces;

public interface IAlbumRepository
{
    Task<List<Album>> GetByUserAsync(string userId);
    Task<Album?> GetByIdAsync(string userId, string albumId);
    Task<Album?> CreateAsync(string userId, Album album);
    Task<bool> UpdateAsync(string userId, Album album);
    Task<bool> DeleteAsync(string userId, string albumId);
}
