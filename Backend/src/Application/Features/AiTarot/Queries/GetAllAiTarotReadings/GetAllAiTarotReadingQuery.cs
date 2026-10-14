using MediatR;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiTarot;

public record GetAllAiTarotReadingQuery(int Page, int PageSize, Guid UserId)
    : IRequest<GetAllAiTarotReadingResult>;

public record GetAllAiTarotReadingResult(
    List<GetAllAiTarotReadingItem> Items,
    int Page,
    int PageSize,
    int Total
) : PaginationResponse<GetAllAiTarotReadingItem>(Items, Page, PageSize, Total);

public record GetAllAiTarotReadingItem(
    Guid Id,
    CardCount CardCount,
    QuestionType Type,
    string Title,
    string AnswerSummary,
    List<AiReadingCard> Cards,
    DateTimeOffset CreatedAt
);
