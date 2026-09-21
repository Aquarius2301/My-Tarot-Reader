namespace MyTarotReader.Application.Constants.Errors;

public class AiTarotErrorCode
{
    private const string Prefix = "error.aiTarot.";

    public const string InvalidCardCount = $"{Prefix}invalidCardCount";
    public const string InvalidCard = $"{Prefix}invalidCard";
    public const string InvalidLocale = $"{Prefix}invalidLocale";
    public const string ReadingNotFound = $"{Prefix}readingNotFound";
    public const string GenerationFailed = $"{Prefix}generationFailed";
}