using Microsoft.Extensions.Configuration;

namespace MyTarotReader.Application.Settings;

/// <summary>
/// A Gemini model with the thinking level requested for it, used by <c>GeminiClient</c>.
/// </summary>
public class GeminiModelSetting
{
    /// <summary>Gemini model id used for readings (e.g. "gemini-3.8-flash").</summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>Thinking level (minimal, low, medium, high); empty or unsupported levels use the model default.</summary>
    public string ThinkingLevel { get; set; } = string.Empty;
}
