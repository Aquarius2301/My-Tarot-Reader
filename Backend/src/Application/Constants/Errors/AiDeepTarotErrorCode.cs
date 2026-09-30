namespace MyTarotReader.Application.Constants.Errors;

public class AiDeepTarotErrorCode
{
    private const string Prefix = "error.aiDeepTarot.";

    public const string InvalidCardCount = $"{Prefix}invalidCardCount";
    public const string InvalidCard = $"{Prefix}invalidCard";
    public const string InvalidLocale = $"{Prefix}invalidLocale";
    public const string InvalidQuestion = $"{Prefix}invalidQuestion";
    public const string InvalidOption = $"{Prefix}invalidOption";
    public const string InvalidTimeFrame = $"{Prefix}invalidTimeFrame";
    public const string ReadingNotFound = $"{Prefix}readingNotFound";
    public const string GenerationFailed = $"{Prefix}generationFailed";
}
