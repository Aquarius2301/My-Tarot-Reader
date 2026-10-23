using System;
using System.Security.Cryptography;

namespace MyTarotReader.Application.Features.Shop.Helpers;

public static class ShopHelper
{
    /// <summary>Generates a unique PayOS order code within 12 digits.</summary>
    public static long GenerateOrderCode()
    {
        var randomNumber = RandomNumberGenerator.GetInt32(0, 100000);
        var prefix = DateTimeOffset.UtcNow.ToString("yyMMdd");
        var baseCode = long.Parse($"{prefix}{randomNumber:D5}");

        return baseCode * 10 + GetLastNumber(baseCode);
    }

    /// <summary>
    /// Validates if the order code is valid
    /// </summary>
    public static bool IsValidOrderCode(long orderCode)
    {
        var lastDigit = (int)(orderCode % 10);
        var baseCode = orderCode / 10;

        return lastDigit == GetLastNumber(baseCode);
    }

    private static int GetLastNumber(long num)
    {
        int sum = 0;
        for (int i = 11; i >= 1; i--)
        {
            int digit = (int)(num % 10);

            sum += digit * (13 - i);

            num /= 10;
        }

        int a = sum % 10;
        int b = a == 0 ? 0 : 10 - a;

        return b;
    }
}
