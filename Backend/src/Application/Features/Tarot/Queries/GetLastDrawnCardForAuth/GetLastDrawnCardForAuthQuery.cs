using MediatR;

namespace MyTarotReader.Application.Features.Tarot;

public record GetLastDrawnCardForAuthQuery(Guid UserId) : IRequest<GetLastDrawnCardForAuthResult?>;

public record GetLastDrawnCardForAuthResult(string CardCode, bool IsReversed);