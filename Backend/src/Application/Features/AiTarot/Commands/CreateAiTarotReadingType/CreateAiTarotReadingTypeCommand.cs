using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record CreateAiTarotReadingTypeCommand(
    string Code,
    string NameEn,
    string NameVi,
    bool IsEnabled
) : IRequest<Guid>;
