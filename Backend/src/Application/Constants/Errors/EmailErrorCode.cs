namespace MyTarotReader.Application.Constants.Errors;

public static class EmailErrorCode
{
    private const string Prefix = "error.email.";

    public const string InvalidAddress = $"{Prefix}invalidAddress";
    public const string SendFailed = $"{Prefix}sendFailed";
}
