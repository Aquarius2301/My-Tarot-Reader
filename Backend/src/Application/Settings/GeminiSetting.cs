namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the Google Gemini API keys and models, bound from the <c>Gemini</c> appsettings section.
/// Every key is paired with every model, and each pair is retried before the client fails over.
/// </summary>
public class GeminiSetting
{
    /// <summary>Extra attempts made on the same (api, model) pair before failing over (0 = single attempt).</summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>Base delay of the exponential backoff between two attempts, in milliseconds.</summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>Total seconds spent retrying before giving up; 0 means no limit.</summary>
    public int MaxTotalWaitSeconds { get; set; } = 30;

    /// <summary>Ordered Gemini API keys, tried from the first one on every call.</summary>
    public List<string> Apis { get; set; } = [];

    /// <summary>Ordered Gemini models, each one tried against every key in <see cref="Apis"/>.</summary>
    public List<GeminiModelSetting> Models { get; set; } = [];
}
