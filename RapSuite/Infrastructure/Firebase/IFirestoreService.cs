using RapSuite.Domain.Music;

namespace RapSuite.Infrastructure.Firebase;

public interface IFirestoreService
{
    // Albums
    Task<List<Album>> GetAlbumsAsync(string userId, string idToken);
    Task<Album?> GetAlbumAsync(string userId, string albumId, string idToken);
    Task<Album?> CreateAlbumAsync(string userId, Album album, string idToken);
    Task<bool> UpdateAlbumAsync(string userId, Album album, string idToken);
    Task<bool> DeleteAlbumAsync(string userId, string albumId, string idToken);

    // Songs
    Task<List<Song>> GetSongsAsync(string userId, string albumId, string idToken);
    Task<Song?> GetSongAsync(string userId, string albumId, string songId, string idToken);
    Task<Song?> CreateSongAsync(string userId, string albumId, Song song, string idToken);
    Task<bool> UpdateSongAsync(string userId, string albumId, Song song, string idToken);
    Task<bool> DeleteSongAsync(string userId, string albumId, string songId, string idToken);

    // User Profile
    Task SaveUserProfileAsync(string userId, AppUser user, string idToken);
}
