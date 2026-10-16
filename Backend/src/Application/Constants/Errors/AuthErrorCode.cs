using System;

namespace MyTarotReader.Application.Constants.Errors;

public static class AuthErrorCode
{
    private const string Prefix = "error.auth.";

    public const string InvalidKeyCredential = $"{Prefix}invalidKeyCredential";
    public const string InvalidRefreshToken = $"{Prefix}invalidRefreshToken";
}
