using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Features.AiTarot;

public class CreateAiTarotReadingTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateAiTarotReadingTypeCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateAiTarotReadingTypeCommand request,
        CancellationToken cancellationToken
    )
    {
        var code = request.Code.Trim().ToLowerInvariant();

        var readingType = await context
            .AITarotReadingTypes.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Code.ToLower() == code, cancellationToken);

        if (readingType is { DeletedAt: null })
        {
            throw new ConflictException(AiTarotErrorCode.ReadingTypeCodeExists);
        }

        if (readingType is null)
        {
            readingType = new AiTarotReadingType();
            context.AITarotReadingTypes.Add(readingType);
        }

        readingType.Code = code;
        readingType.NameEn = request.NameEn;
        readingType.NameVi = request.NameVi;
        readingType.IsEnabled = request.IsEnabled;
        readingType.DeletedAt = null;

        await context.SaveChangesAsync(cancellationToken);

        return readingType.Id;
    }
}
