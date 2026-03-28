using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using RapSuite.Domain.Interfaces;
using RapSuite.Domain.Models;

namespace RapSuite.Infrastructure.AI;

public class NvidiaLyricsAiService : ILyricsAiService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<NvidiaLyricsAiService> _logger;

    public NvidiaLyricsAiService(IConfiguration configuration, ILogger<NvidiaLyricsAiService> logger)
    {
        var apiKey = configuration["Nvidia:ApiKey"]
            ?? throw new InvalidOperationException("Nvidia:ApiKey is not configured");
        var model = configuration["Nvidia:Model"] ?? "meta/llama-3.1-405b-instruct";

        _logger = logger;

        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri("https://integrate.api.nvidia.com/v1") });

        _chatClient = openAiClient.GetChatClient(model).AsIChatClient();
    }

    public async Task<LyricsResult> GenerateLyricsAsync(GenerationRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = PromptTemplateService.BuildGenerationPrompt(
            request.Situation, request.Language, request.Mood, request.TargetDurationMinutes);

        return await CallAiAsync(prompt, cancellationToken);
    }

    public async Task<LyricsResult> RephraseLyricsAsync(RephraseRequest request, CancellationToken cancellationToken = default)
    {
        var prompt = PromptTemplateService.BuildRephrasePrompt(
            request.OriginalLyrics, request.Language, request.Mood, request.Style);

        return await CallAiAsync(prompt, cancellationToken);
    }

    private async Task<LyricsResult> CallAiAsync(string prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, "You are a world-class songwriter and rapper. You create powerful, authentic lyrics with strong rhythm, rhyme, and emotional depth."),
                new(ChatRole.User, prompt)
            };

            var options = new ChatOptions
            {
                Temperature = 0.8f,
                TopP = 0.95f,
                MaxOutputTokens = 4096
            };

            var response = await _chatClient.GetResponseAsync(messages, options, cancellationToken);
            var lyricsText = response.Text ?? "";

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
        catch (OperationCanceledException)
        {
            throw;
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
            else if (!line.StartsWith("WORD_COUNT:", StringComparison.OrdinalIgnoreCase))
            {
                lyricsLines.Add(line);
            }
        }

        return (title, string.Join("\n", lyricsLines).Trim());
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.AsSpan().ToString()
            .Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries)
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
