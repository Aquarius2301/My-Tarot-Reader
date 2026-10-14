using MediatR;

namespace MyTarotReader.Application.Features.Tarot;

public record CreateDrawForGuestCommand(string CardCode, bool IsReversed, string GuestKey)
    : IRequest;

record DrawRecord(long DrawnAtUnixSeconds, string CardCode, bool IsReversed);
