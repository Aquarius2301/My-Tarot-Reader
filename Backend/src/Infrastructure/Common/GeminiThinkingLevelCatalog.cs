namespace MyTarotReader.Infrastructure.Common;

/// <summary>
/// Thinking levels accepted by each Gemini model, so an unsupported level is never sent (the API answers 400).
/// Models absent from the catalog - the 2.5 series included, which only accepts <c>thinkingBudget</c> - are called
/// without a <c>thinkingConfig</c> so the model default applies.
/// </summary>
public static class GeminiThinkingLevelCatalog
{
    private static readonly Dictionary<string, string[]> SupportedByModel = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        ["gemini-3.8-flash"] = ["low", "medium", "high"],
        ["gemini-3.7-flash"] = ["low", "medium", "high"],
        ["gemini-3.6-flash"] = ["minimal", "low", "medium", "high"],
        ["gemini-3.5-flash"] = ["minimal", "low", "medium", "high"],
        ["gemini-3.5-flash-lite"] = ["minimal", "low", "medium", "high"],
        ["gemini-3.1-pro-preview"] = ["low", "medium", "high"],
        ["gemini-3.1-flash-lite"] = ["minimal", "low", "medium", "high"],
        ["gemini-3.1-flash-lite-image"] = ["minimal", "high"],
        ["gemini-3-flash-preview"] = ["minimal", "low", "medium", "high"],
        ["gemini-3-pro-preview"] = ["low", "high"],
    };

    /// <summary>
    /// Returns the thinking level to send for a model, or null when the model or the level is unsupported.
    /// </summary>
    /// <param name="model">The Gemini model id configured for the call.</param>
    /// <param name="thinkingLevel">The thinking level configured for the model.</param>
    public static string? Resolve(string? model, string? thinkingLevel)
    {
        if (string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(thinkingLevel))
        {
            return null;
        }

        if (!SupportedByModel.TryGetValue(model.Trim(), out var supported))
        {
            return null;
        }

        var level = thinkingLevel.Trim().ToLowerInvariant();
        return Array.IndexOf(supported, level) >= 0 ? level : null;
    }
}
