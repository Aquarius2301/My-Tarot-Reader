using System;

namespace MyTarotReader.Application.Common.Time;

public class VietnamTimeProvider : ITimeProvider
{
    private static readonly TimeZoneInfo VietnamTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

    public DateTimeOffset GetVietnamNow()
    {
        var utcNow = DateTimeOffset.UtcNow;
        return TimeZoneInfo.ConvertTime(utcNow, VietnamTimeZone);
    }
}