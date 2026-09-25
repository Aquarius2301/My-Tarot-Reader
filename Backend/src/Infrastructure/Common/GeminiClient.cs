using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Common;

/// <summary>
/// Sends generateContent requests to the Google Gemini REST API using a raw HttpClient
/// (no external SDK), configured from <see cref="GeminiSetting"/>.
/// </summary>
public class GeminiClient(HttpClient httpClient, IOptions<GeminiSetting> setting) : IGeminiClient
{
    private const int MaxAttempts = 3;

    private readonly HttpClient _httpClient = httpClient;
    private readonly GeminiSetting _setting = setting.Value;

    /// <inheritdoc/>
    public async Task<string> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default
    )
    {
        var apis = _setting
            .Apis.Where(a => !string.IsNullOrWhiteSpace(a.ApiKey) && !string.IsNullOrWhiteSpace(a.Model))
            .ToList();

        if (apis.Count == 0)
        {
            throw new InternalServerException(AiTarotErrorCode.GenerationFailed);
        }

        // Pick a random start, then try up to MaxAttempts consecutive credentials
        // in circular order so every configured API gets used over time.
        var start = Random.Shared.Next(apis.Count);
        var candidates = apis
            .Skip(start)
            .Concat(apis.Take(start))
            .Take(MaxAttempts)
            .ToList();

        Exception? lastException = null;
        foreach (var api in candidates)
        {
            try
            {
                return await TryCallAsync(api, prompt, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or InternalServerException)
            {
                lastException = ex;
            }
        }

        throw new InternalServerException(
            AiTarotErrorCode.GenerationFailed,
            innerException: lastException
        );
    }

    private async Task<string> TryCallAsync(
        GeminiApiSetting api,
        string prompt,
        CancellationToken cancellationToken
    )
    {
        var request = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } },
                },
            },
            generationConfig = new { responseMimeType = "application/json" },
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{api.Model}:generateContent";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Add("x-goog-api-key", api.ApiKey);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Gemini API returned {(int)response.StatusCode}.",
                inner: null,
                statusCode: response.StatusCode
            );
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (
            !root.TryGetProperty("candidates", out var candidates)
            || candidates.GetArrayLength() == 0
            || !candidates[0].TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var parts)
            || parts.GetArrayLength() == 0
            || !parts[0].TryGetProperty("text", out var text)
        )
        {
            throw new InternalServerException(AiTarotErrorCode.GenerationFailed);
        }

        var result = text.GetString();
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InternalServerException(AiTarotErrorCode.GenerationFailed);
        }

        return result;
    }
}