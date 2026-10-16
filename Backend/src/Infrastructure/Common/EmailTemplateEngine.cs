using System.Collections.Concurrent;
using MyTarotReader.Application.Contracts.Common;

namespace MyTarotReader.Infrastructure.Common;

public class EmailTemplateEngine : IEmailTemplateEngine
{
    private static readonly ConcurrentDictionary<string, string> TemplateCache = new();

    public string Render(string resourceName, IDictionary<string, string> replacements)
    {
        var template = TemplateCache.GetOrAdd(resourceName, LoadResource);

        foreach (var (key, value) in replacements)
        {
            template = template.Replace($"{{{{{key}}}}}", value);
        }

        return template;
    }

    private static string LoadResource(string resourceName)
    {
        using var stream =
            typeof(EmailTemplateEngine).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found."
            );

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
