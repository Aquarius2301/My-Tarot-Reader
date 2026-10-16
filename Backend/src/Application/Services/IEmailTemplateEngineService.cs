namespace MyTarotReader.Application.Services;

public interface IEmailTemplateEngineService
{
    string Render(string resourceName, IDictionary<string, string> replacements);
}
