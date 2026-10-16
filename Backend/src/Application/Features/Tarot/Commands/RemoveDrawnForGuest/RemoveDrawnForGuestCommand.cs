using MediatR;

namespace MyTarotReader.Application.Features.Tarot;

public record RemoveDrawnForGuestCommand(string GuestId) : IRequest;
