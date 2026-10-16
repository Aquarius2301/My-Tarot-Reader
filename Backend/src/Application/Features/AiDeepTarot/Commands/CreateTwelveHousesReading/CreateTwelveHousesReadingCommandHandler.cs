using MediatR;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class CreateTwelveHousesReadingCommandHandler(
    IAppDbContext context,
    IGeminiClientService geminiClientService,
    IWalletService walletService
) : CreateDeepTarotReadingHandlerBase(context, geminiClientService, walletService),
    IRequestHandler<CreateTwelveHousesReadingCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateTwelveHousesReadingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var topic = DeepTarotTopic.TwelveHouses;
        var cost = DeepTarotConstant.GetCost(topic);
        var positions = DeepTarotConstant.GetPositions(topic);

        var prompt = AiDeepTarotHelper.BuildPrompt(
            topic,
            request.Locale,
            request.Cards,
            positions,
            DateTimeOffset.UtcNow
        );

        return await CreateReadingAsync(
            topic,
            prompt,
            positions,
            cost,
            request.Cards,
            request.UserId,
            cancellationToken
        );
    }
}
