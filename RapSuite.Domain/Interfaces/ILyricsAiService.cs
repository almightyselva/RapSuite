using RapSuite.Domain.Models;

namespace RapSuite.Domain.Interfaces;

public interface ILyricsAiService
{
    Task<LyricsResult> GenerateLyricsAsync(GenerationRequest request, CancellationToken cancellationToken = default);
    Task<LyricsResult> RephraseLyricsAsync(RephraseRequest request, CancellationToken cancellationToken = default);
}
