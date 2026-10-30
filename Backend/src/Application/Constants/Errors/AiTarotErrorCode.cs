namespace MyTarotReader.Application.Constants.Errors;

public class AiTarotErrorCode
{
    private const string Prefix = "error.aiTarot.";

    public const string InvalidCardCount = $"{Prefix}invalidCardCount";
    public const string InvalidCard = $"{Prefix}invalidCard";
    public const string InvalidQuestionType = $"{Prefix}invalidQuestionType";
    public const string InvalidLocale = $"{Prefix}invalidLocale";
    public const string ReadingNotFound = $"{Prefix}readingNotFound";
    public const string ReadingCountNotFound = $"{Prefix}readingCountNotFound";
    public const string ReadingTypeNotFound = $"{Prefix}readingTypeNotFound";
    public const string CardCountExists = $"{Prefix}cardCountExists";
    public const string ReadingTypeCodeExists = $"{Prefix}readingTypeCodeExists";
    public const string InvalidReadingCount = $"{Prefix}invalidReadingCount";
    public const string InvalidReadingType = $"{Prefix}invalidReadingType";
    public const string GenerationFailed = $"{Prefix}generationFailed";
}