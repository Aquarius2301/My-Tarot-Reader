namespace MyTarotReader.Application.Constants.Errors;

public static class SystemErrorCode
{
    private const string Prefix = "error.system.";

    public const string InternalServerError = $"{Prefix}internalServerError";
    public const string BadRequest = $"{Prefix}badRequest";
    public const string Unauthorized = $"{Prefix}unauthorized";
    public const string Forbidden = $"{Prefix}forbidden";
    public const string NotFound = $"{Prefix}notFound";
    public const string Conflict = $"{Prefix}conflict";
    public const string RequestAborted = $"{Prefix}requestAborted";
    public const string TooManyRequests = $"{Prefix}tooManyRequests";
}
