using MediatR;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class CreateCrossroadsReadingCommandHandler(
    IAppDbContext context,
    IGeminiClientService geminiClientService,
    IWalletService walletService
) : CreateDeepTarotReadingHandlerBase(context, geminiClientService, walletService),
    IRequestHandler<CreateCrossroadsReadingCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateCrossroadsReadingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        AiDeepTarotHelper.GuardCrossroadsInput(request.Question, request.Options);

        var optionCount = request.Options.Count;
        var positions = DeepTarotConstant.GetCrossroadsPositions(optionCount);
        var cost = DeepTarotConstant.GetCost(DeepTarotTopic.Crossroads, optionCount);

        var prompt = AiDeepTarotHelper.BuildCrossroadsPrompt(
            request.Locale,
            request.Question,
            request.Options,
            request.TimeFrame,
            request.Cards,
            positions
        );

        return await CreateReadingAsync(
            DeepTarotTopic.Crossroads,
            prompt,
            positions,
            cost,
            request.Cards,
            request.UserId,
            cancellationToken,
            question: request.Question,
            options: request.Options,
            timeFrame: request.TimeFrame
        );
    }
}
