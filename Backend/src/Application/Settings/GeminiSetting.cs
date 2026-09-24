namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for Google Gemini API credentials. Bound from the <c>Gemini</c> appsettings
/// section. May hold many entries; a single call only tries up to three.
/// </summary>
public class GeminiSetting
{
    /// <summary>
    /// Ordered fallback list of Gemini credentials. May contain any number of entries,
    /// but <c>GeminiClient</c> uses only the first three valid ones per call.
    /// </summary>
    public List<GeminiApiSetting> Apis { get; set; } = [];
}