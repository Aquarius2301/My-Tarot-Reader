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

    /// <summary>
    /// White coin cost of an AI reading, keyed by the number of cards
    /// (3 → 2, 5 → 3, 7 → 4, 10 → 5).
    /// </summary>
    public Dictionary<int, int> Costs { get; set; } =
        new()
        {
            [3] = 2,
            [5] = 3,
            [7] = 4,
            [10] = 5,
        };
}
