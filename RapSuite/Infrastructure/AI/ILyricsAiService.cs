using RapSuite.Domain.Lyrics;

namespace RapSuite.Infrastructure.AI;

public interface ILyricsAiService
{
    Task<LyricsResult> GenerateLyricsAsync(GenerationRequest request);
    Task<LyricsResult> RephraseLyricsAsync(RephraseRequest request);
}
