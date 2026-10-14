using MediatR;

namespace MyTarotReader.Application.Features.Tarot;

public record GetLastDrawnCardForGuestQuery(string GuestKey)
    : IRequest<GetLastDrawnCardForGuestResult?>;

public record GetLastDrawnCardForGuestResult(
    string CardCode,
    bool IsReversed,
    long RemainingSeconds
);
