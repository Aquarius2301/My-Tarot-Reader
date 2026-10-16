using System.Collections.Concurrent;
using MyTarotReader.Application.Services;

namespace MyTarotReader.Infrastructure.Services;

public class EmailTemplateEngineService : IEmailTemplateEngineService
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
            typeof(EmailTemplateEngineService).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{resourceName}' not found."
            );

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
