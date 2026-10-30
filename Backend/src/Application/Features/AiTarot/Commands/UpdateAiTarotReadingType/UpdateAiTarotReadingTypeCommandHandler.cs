using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class UpdateAiTarotReadingTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateAiTarotReadingTypeCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateAiTarotReadingTypeCommand request,
        CancellationToken cancellationToken
    )
    {
        var readingType =
            await context.AITarotReadingTypes.FirstOrDefaultAsync(
                s => s.Id == request.Id,
                cancellationToken
            ) ?? throw new NotFoundException(AiTarotErrorCode.ReadingTypeNotFound);

        var code = request.Code.Trim().ToLowerInvariant();

        var codeExists = await context
            .AITarotReadingTypes.IgnoreQueryFilters()
            .AnyAsync(
                s => s.Code.ToLower() == code && s.Id != request.Id,
                cancellationToken
            );

        if (codeExists)
        {
            throw new ConflictException(AiTarotErrorCode.ReadingTypeCodeExists);
        }

        readingType.Code = code;
        readingType.NameEn = request.NameEn;
        readingType.NameVi = request.NameVi;
        readingType.IsEnabled = request.IsEnabled;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
