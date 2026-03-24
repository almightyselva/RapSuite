namespace RapSuite.Domain.Lyrics;

public class GenerationRequest
{
    public string Situation { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public string Mood { get; set; } = "Energetic";
    public int TargetDurationMinutes { get; set; } = 3;
}

public class RephraseRequest
{
    public string OriginalLyrics { get; set; } = string.Empty;
    public string Language { get; set; } = "English";
    public string Mood { get; set; } = "Energetic";
    public string Style { get; set; } = "Rap";
}

public class LyricsResult
{
    public string Title { get; set; } = string.Empty;
    public string Lyrics { get; set; } = string.Empty;
    public int WordCount { get; set; }
    public string EstimatedDuration { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
