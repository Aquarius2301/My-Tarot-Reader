namespace MyTarotReader.Application.Constants.Errors;

public class AiDeepTarotErrorCode
{
    private const string Prefix = "error.aiDeepTarot.";

    public const string InvalidTopic = $"{Prefix}invalidTopic";
    public const string TopicNotSupported = $"{Prefix}topicNotSupported";
    public const string InvalidCardCount = $"{Prefix}invalidCardCount";
    public const string InvalidCard = $"{Prefix}invalidCard";
    public const string InvalidLocale = $"{Prefix}invalidLocale";
    public const string ReadingNotFound = $"{Prefix}readingNotFound";
    public const string GenerationFailed = $"{Prefix}generationFailed";
}
