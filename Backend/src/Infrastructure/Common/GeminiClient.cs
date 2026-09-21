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
/// (no external SDK), configured from <see cref="AiTarotSetting"/>.
/// </summary>
public class GeminiClient(HttpClient httpClient, IOptions<AiTarotSetting> setting) : IGeminiClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly AiTarotSetting _setting = setting.Value;

    /// <inheritdoc/>
    public async Task<string> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_setting.ApiKey))
            {
                throw new InternalServerException(AiTarotErrorCode.GenerationFailed);
            }

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

            var url =
                $"https://generativelanguage.googleapis.com/v1beta/models/{_setting.Model}:generateContent";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Headers.Add("x-goog-api-key", _setting.ApiKey);
            httpRequest.Content = JsonContent.Create(request);

            using var response = await _httpClient.SendAsync(
                httpRequest,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

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
        catch (InternalServerException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException(
                AiTarotErrorCode.GenerationFailed,
                innerException: ex
            );
        }
    }
}