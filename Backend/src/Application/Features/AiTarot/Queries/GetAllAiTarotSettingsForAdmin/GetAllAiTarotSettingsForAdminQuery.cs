using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record GetAllAiTarotSettingsForAdminQuery : IRequest<GetAllAiTarotSettingsForAdminResult>;

/// <summary> A spread configuration as shown to admins, including disabled spreads. </summary>
public record AdminAiTarotReadingCountItem(
    Guid Id,
    int CardCount,
    int WhiteCoins,
    List<string> PositionsEn,
    List<string> PositionsVi,
    int MinWords,
    int MaxWords,
    bool IsEnabled
);

/// <summary> A question type as shown to admins, including disabled types. </summary>
public record AdminAiTarotReadingTypeItem(
    Guid Id,
    string Code,
    string NameEn,
    string NameVi,
    bool IsEnabled
);

/// <summary>Result of listing every AI tarot setting for admins.</summary>
public record GetAllAiTarotSettingsForAdminResult(
    List<AdminAiTarotReadingCountItem> ReadingCounts,
    List<AdminAiTarotReadingTypeItem> ReadingTypes
);
