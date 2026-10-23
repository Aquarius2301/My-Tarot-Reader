namespace MyTarotReader.Application.Common.Time;

public interface ITimeProvider
{
    DateTimeOffset GetVietnamNow();
}