using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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
public class GeminiClient(
    HttpClient httpClient,
    IOptions<GeminiSetting> setting,
    ILogger<GeminiClient> logger
) : IGeminiClient
{
    private const int MaxBackoffMilliseconds = 60_000;
    private const int MaxRetryAfterSeconds = 60;

    private readonly HttpClient _httpClient = httpClient;
    private readonly GeminiSetting _setting = setting.Value;
    private readonly ILogger<GeminiClient> _logger = logger;

    /// <inheritdoc/>
    public async Task<string> GenerateContentAsync(
        string prompt,
        CancellationToken cancellationToken = default
    )
    {
        var apis = _setting
            .Apis.Where(api => !string.IsNullOrWhiteSpace(api))
            .Select(api => api.Trim())
            .ToList();
        var models = _setting
            .Models.Where(model => !string.IsNullOrWhiteSpace(model.Model))
            .ToList();

        if (apis.Count == 0 || models.Count == 0)
        {
            throw new InternalServerException(AiTarotErrorCode.GenerationFailed);
        }

        var stopwatch = Stopwatch.StartNew();
        var visited = new HashSet<(int Api, int Model)>();
        var apiIndex = 0;
        var modelIndex = 0;
        Exception? lastException = null;

        while (visited.Add((apiIndex, modelIndex)))
        {
            var attempt = await TryPairAsync(
                apis[apiIndex],
                models[modelIndex],
                prompt,
                stopwatch,
                cancellationToken
            );

            if (attempt.Text is not null)
            {
                return attempt.Text;
            }

            lastException = attempt.Exception;
            (apiIndex, modelIndex) = NextPair(
                apiIndex,
                modelIndex,
                apis.Count,
                models.Count,
                attempt.Kind,
                visited
            );
        }

        throw new InternalServerException(
            AiTarotErrorCode.GenerationFailed,
            innerException: lastException
        );
    }

    private async Task<AttemptResult> TryPairAsync(
        string api,
        GeminiModelSetting model,
        string prompt,
        Stopwatch stopwatch,
        CancellationToken cancellationToken
    )
    {
        var maxRetries = Math.Max(0, _setting.MaxRetries);
        var attempt = await SendAsync(api, model, prompt, cancellationToken);

        for (
            var retry = 0;
            attempt.Text is null && IsRetryable(attempt.Kind) && retry < maxRetries;
            retry++
        )
        {
            if (IsWaitBudgetSpent(stopwatch))
            {
                _logger.LogWarning(
                    "Gemini retry budget of {MaxTotalWaitSeconds}s is spent after {ElapsedMs}ms, stopping retries on model {Model}.",
                    _setting.MaxTotalWaitSeconds,
                    stopwatch.ElapsedMilliseconds,
                    model.Model
                );
                break;
            }

            var delay = ComputeDelay(retry, attempt.RetryAfter);
            _logger.LogWarning(
                "Gemini call failed ({Kind}, HTTP {StatusCode}) for model {Model}, retrying in {DelayMs}ms ({Retry}/{MaxRetries}).",
                attempt.Kind,
                attempt.StatusCode,
                model.Model,
                delay.TotalMilliseconds,
                retry + 1,
                maxRetries
            );

            await DelayAsync(delay, cancellationToken);
            attempt = await SendAsync(api, model, prompt, cancellationToken);
        }

        return attempt;
    }

    private async Task<AttemptResult> SendAsync(
        string api,
        GeminiModelSetting model,
        string prompt,
        CancellationToken cancellationToken
    )
    {
        var generationConfig = new Dictionary<string, object?>
        {
            ["responseMimeType"] = "application/json",
        };

        var thinkingLevel = GeminiThinkingLevelCatalog.Resolve(model.Model, model.ThinkingLevel);
        if (thinkingLevel is null)
        {
            if (!string.IsNullOrWhiteSpace(model.ThinkingLevel))
            {
                _logger.LogWarning(
                    "Model {Model} does not support thinking level {ThinkingLevel}, calling it with the model default.",
                    model.Model,
                    model.ThinkingLevel
                );
            }
        }
        else
        {
            generationConfig["thinkingConfig"] = new { thinkingLevel };
        }

        var body = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig,
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model.Model}:generateContent";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Add("x-goog-api-key", api);
        httpRequest.Content = JsonContent.Create(body);

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new AttemptResult(
                    Text: null,
                    Classify(response.StatusCode),
                    new HttpRequestException(
                        $"Gemini API returned {(int)response.StatusCode}.",
                        inner: null,
                        statusCode: response.StatusCode
                    ),
                    response.StatusCode,
                    response.Headers.RetryAfter?.Delta
                );
            }

            return ReadResult(await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (HttpRequestException ex)
        {
            return new AttemptResult(null, FailureKind.ServerError, ex, null, null);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new AttemptResult(null, FailureKind.ServerError, ex, null, null);
        }
    }

    private static AttemptResult ReadResult(string json)
    {
        try
        {
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
                return InvalidResult();
            }

            var result = text.GetString();
            return string.IsNullOrWhiteSpace(result)
                ? InvalidResult()
                : new AttemptResult(result, FailureKind.None, null, null, null);
        }
        catch (JsonException ex)
        {
            return new AttemptResult(
                null,
                FailureKind.InvalidResponse,
                new InternalServerException(AiTarotErrorCode.GenerationFailed, innerException: ex),
                null,
                null
            );
        }
    }

    private static AttemptResult InvalidResult() =>
        new(
            null,
            FailureKind.InvalidResponse,
            new InternalServerException(AiTarotErrorCode.GenerationFailed),
            null,
            null
        );

    private static (int Api, int Model) NextPair(
        int apiIndex,
        int modelIndex,
        int apiCount,
        int modelCount,
        FailureKind kind,
        HashSet<(int Api, int Model)> visited
    )
    {
        var switchApi = kind is FailureKind.RateLimited or FailureKind.ApiUnavailable;
        var preferred = switchApi
            ? ((apiIndex + 1) % apiCount, modelIndex)
            : (apiIndex, (modelIndex + 1) % modelCount);

        if (!visited.Contains(preferred))
        {
            return preferred;
        }

        for (var api = 0; api < apiCount; api++)
        {
            for (var model = 0; model < modelCount; model++)
            {
                if (!visited.Contains((api, model)))
                {
                    return (api, model);
                }
            }
        }

        return preferred;
    }

    private TimeSpan ComputeDelay(int retry, TimeSpan? retryAfter)
    {
        var baseMilliseconds = Math.Max(0, _setting.RetryDelayMilliseconds);
        var exponential = retry == 0 ? baseMilliseconds : baseMilliseconds * Math.Pow(2, retry);
        var jitter = retry == 0 ? 0 : Random.Shared.Next(baseMilliseconds);
        var delay = Math.Min(exponential + jitter, MaxBackoffMilliseconds);

        if (retryAfter is { } requested && requested > TimeSpan.FromMilliseconds(delay))
        {
            delay = Math.Min(requested.TotalMilliseconds, MaxRetryAfterSeconds * 1000d);
        }

        return TimeSpan.FromMilliseconds(delay);
    }

    private bool IsWaitBudgetSpent(Stopwatch stopwatch) =>
        _setting.MaxTotalWaitSeconds > 0
        && stopwatch.Elapsed.TotalSeconds >= _setting.MaxTotalWaitSeconds;

    private static bool IsRetryable(FailureKind kind) =>
        kind is FailureKind.RateLimited or FailureKind.ServerError;

    private static FailureKind Classify(HttpStatusCode statusCode) =>
        statusCode switch
        {
            HttpStatusCode.TooManyRequests => FailureKind.RateLimited,
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => FailureKind.ApiUnavailable,
            HttpStatusCode.BadRequest or HttpStatusCode.NotFound => FailureKind.ModelUnavailable,
            HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout => FailureKind.ServerError,
            _ => FailureKind.Unknown,
        };

    /// <summary>
    /// Waits before the next attempt; overridden in tests so they never sleep for real.
    /// </summary>
    /// <param name="delay">How long to wait.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected virtual Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask;

    private enum FailureKind
    {
        None,
        RateLimited,
        ApiUnavailable,
        ModelUnavailable,
        ServerError,
        InvalidResponse,
        Unknown,
    }

    private sealed record AttemptResult(
        string? Text,
        FailureKind Kind,
        Exception? Exception,
        HttpStatusCode? StatusCode,
        TimeSpan? RetryAfter
    );
}
