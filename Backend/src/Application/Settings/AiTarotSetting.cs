namespace MyTarotReader.Application.Settings;

/// <summary>Configuration for the AI tarot reading feature.</summary>
public class AiTarotSetting
{
    /// <summary>
    /// Google Gemini API key used to authenticate generateContent calls.
    /// Bound from the <c>AiTarot:ApiKey</c> appsettings section.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gemini model id used for readings (e.g. "gemini-2.0-flash").
    /// Bound from the <c>AiTarot:Model</c> appsettings section.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of tokens Gemini may generate per response.
    /// Bound from the <c>AiTarot:MaxOutputTokens</c> appsettings section.
    /// </summary>
    public int MaxOutputTokens { get; set; } = 16384;
}
