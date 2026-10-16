namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the AI tarot reading feature.
/// Bound from the <c>AiTarot</c> appsettings section.
/// </summary>
public class AiTarotSetting
{
    /// <summary>Google Gemini API key used to authenticate generateContent calls.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gemini model id used for readings (e.g. "gemini-2.0-flash").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Maximum number of tokens Gemini may generate per response.</summary>
    public int MaxOutputTokens { get; set; } = 16384;
}
