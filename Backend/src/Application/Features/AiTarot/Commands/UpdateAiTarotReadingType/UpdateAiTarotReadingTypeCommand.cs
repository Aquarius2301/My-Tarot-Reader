using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record UpdateAiTarotReadingTypeCommand(
    Guid Id,
    string Code,
    string NameEn,
    string NameVi,
    bool IsEnabled
) : IRequest<Unit>;
