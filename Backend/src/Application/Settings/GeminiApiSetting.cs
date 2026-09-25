namespace MyTarotReader.Application.Settings;

/// <summary>
/// A single Gemini API credential (API key + model id) used by <c>GeminiClient</c>.
/// </summary>
public class GeminiApiSetting
{
    /// <summary>Google Gemini API key used to authenticate generateContent calls.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Gemini model id used for readings (e.g. "gemini-2.0-flash").</summary>
    public string Model { get; set; } = string.Empty;
}