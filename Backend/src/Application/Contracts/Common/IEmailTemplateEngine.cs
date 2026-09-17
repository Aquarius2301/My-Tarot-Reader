namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Renders email templates by substituting placeholders with provided values.
/// </summary>
public interface IEmailTemplateEngine
{
    /// <summary>
    /// Renders an email template from the specified resource name, replacing placeholders with the provided values.
    /// </summary>
    /// <param name="resourceName">The name of the resource containing the email template.</param>
    /// <param name="replacements">A dictionary containing placeholder keys and their corresponding replacement values.</param>
    /// <returns>The rendered email template as a string.</returns>
    string Render(string resourceName, IDictionary<string, string> replacements);
}
