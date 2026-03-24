using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RapSuite.Domain.Lyrics;

namespace RapSuite.Infrastructure.AI;

public class NvidiaLyricsAiService : ILyricsAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly ILogger<NvidiaLyricsAiService> _logger;

    public NvidiaLyricsAiService(HttpClient httpClient, IConfiguration configuration, ILogger<NvidiaLyricsAiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Nvidia:ApiKey"]
            ?? throw new InvalidOperationException("Nvidia:ApiKey is not configured");
        _model = configuration["Nvidia:Model"] ?? "meta/llama-3.1-405b-instruct";
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://integrate.api.nvidia.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<LyricsResult> GenerateLyricsAsync(GenerationRequest request)
    {
        var prompt = PromptTemplateService.BuildGenerationPrompt(
            request.Situation, request.Language, request.Mood, request.TargetDurationMinutes);

        return await CallAiAsync(prompt);
    }

    public async Task<LyricsResult> RephraseLyricsAsync(RephraseRequest request)
    {
        var prompt = PromptTemplateService.BuildRephrasePrompt(
            request.OriginalLyrics, request.Language, request.Mood, request.Style);

        return await CallAiAsync(prompt);
    }

    private async Task<LyricsResult> CallAiAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a world-class songwriter and rapper. You create powerful, authentic lyrics with strong rhythm, rhyme, and emotional depth." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.8,
                top_p = 0.95,
                max_tokens = 4096
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("NVIDIA API error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                return new LyricsResult
                {
                    Success = false,
                    ErrorMessage = $"AI service returned {response.StatusCode}. Please try again."
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);

            var lyricsText = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "";

            var (title, lyrics) = ParseResponse(lyricsText);
            var wordCount = CountWords(lyrics);
            var estimatedDuration = EstimateDuration(wordCount);

            return new LyricsResult
            {
                Title = title,
                Lyrics = lyrics,
                WordCount = wordCount,
                EstimatedDuration = estimatedDuration,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling NVIDIA AI service");
            return new LyricsResult
            {
                Success = false,
                ErrorMessage = "Failed to generate lyrics. Please check your connection and try again."
            };
        }
    }

    private static (string Title, string Lyrics) ParseResponse(string response)
    {
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var title = "Untitled";
        var lyricsLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                title = line["TITLE:".Length..].Trim();
            }
            else if (line.StartsWith("WORD_COUNT:", StringComparison.OrdinalIgnoreCase))
            {
                // Skip metadata
            }
            else
            {
                lyricsLines.Add(line);
            }
        }

        return (title, string.Join("\n", lyricsLines).Trim());
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Count(w => !w.StartsWith('[') || !w.EndsWith(']'));
    }

    private static string EstimateDuration(int wordCount)
    {
        var minutes = wordCount / 140.0;
        var mins = (int)minutes;
        var secs = (int)((minutes - mins) * 60);
        return $"{mins}:{secs:D2}";
    }
}
