using System;

namespace MyTarotReader.Application.Constants.Errors;

public class TarotReadingErrorCode
{
    private const string Prefix = "error.tarot.";

    public const string InvalidCardCode = $"{Prefix}invalidCardCode";
    public const string InvalidGuestKey = $"{Prefix}invalidGuestKey";
    public const string DrawnAlready = $"{Prefix}drawnAlready";
    public const string NotFound = $"{Prefix}notFound";
}
