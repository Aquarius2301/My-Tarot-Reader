using System;
using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record GetAiTarotSettingQuery : IRequest<GetAiTarotSettingResult>;

public record GetAiTarotSettingResult(
    List<AiTarotReadingCountItem> ReadingCounts,
    List<AiTarotReadingTypeItem> ReadingTypes
);

public record AiTarotReadingCountItem(
    int CardCount,
    int WhiteCoins,
    List<string> PositionsEn,
    List<string> PositionsVi
);

public record AiTarotReadingTypeItem(string Code, string NameEn, string NameVi);
