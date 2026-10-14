using MediatR;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class CreateTwelveMonthsReadingCommandHandler(
    IAppDbContext context,
    IGeminiClientService geminiClientService,
    IWalletService walletService
) : CreateDeepTarotReadingHandlerBase(context, geminiClientService, walletService),
    IRequestHandler<CreateTwelveMonthsReadingCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateTwelveMonthsReadingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var topic = DeepTarotTopic.TwelveMonths;
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
