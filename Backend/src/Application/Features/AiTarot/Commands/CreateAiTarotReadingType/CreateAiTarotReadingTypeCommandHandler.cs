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
        var codeExists = await context
            .AITarotReadingTypes.IgnoreQueryFilters()
            .AnyAsync(s => s.Code == request.Code, cancellationToken);

        if (codeExists)
        {
            throw new ConflictException(AiTarotErrorCode.ReadingTypeCodeExists);
        }

        var readingType = new AiTarotReadingType
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameVi = request.NameVi,
            IsEnabled = request.IsEnabled,
        };

        context.AITarotReadingTypes.Add(readingType);
        await context.SaveChangesAsync(cancellationToken);

        return readingType.Id;
    }
}
