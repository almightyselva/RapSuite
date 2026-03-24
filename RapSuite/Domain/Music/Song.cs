namespace RapSuite.Domain.Music;

public class Song
{
    public string Id { get; set; } = string.Empty;
    public string AlbumId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Lyrics { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public string Mood { get; set; } = "Energetic";
    public string GenerationType { get; set; } = "new";
    public int WordCount { get; set; }
    public string EstimatedDuration { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
