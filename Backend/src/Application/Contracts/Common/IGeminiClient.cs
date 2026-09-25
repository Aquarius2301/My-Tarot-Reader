namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Sends prompts to Google Gemini and returns the generated text.
/// </summary>
public interface IGeminiClient
{
    /// <summary>
    /// Sends a prompt to Google Gemini and returns the generated text part.
    /// </summary>
    /// <param name="prompt">The prompt to send to the model.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated text from the model.</returns>
    /// <exception cref="InternalServerException">Thrown when the Gemini API call fails or returns no content.</exception>
    Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken = default);
}