using RapSuite.Domain.Models;

namespace RapSuite.Domain.Interfaces;

public interface ILyricsAiService
{
    Task<LyricsResult> GenerateLyricsAsync(GenerationRequest request);
    Task<LyricsResult> RephraseLyricsAsync(RephraseRequest request);
}
