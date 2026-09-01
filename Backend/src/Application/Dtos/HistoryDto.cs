namespace MyTarotReader.Application.Dtos;

/// <summary>
/// Represents a response containing a list of tarot card read history items.
/// </summary>
/// <param name="Histories">A list of <see cref="HistoryResult"/> objects representing the user's read history.</param>
public record GetHistoryResponse(List<HistoryResult> Histories);

/// <summary>
/// Represents a data transfer object for reading tarot card history.
/// </summary>
/// <param name="Id">The unique identifier for the history entry.</param>
/// <param name="CardCode">The code representing the tarot card.</param>
/// <param name="IsReversed">Indicates if the card was drawn in a reversed position.</param>
/// <param name="CreatedAt">The date and time when the history entry was created.</param>
public record HistoryResult(Guid Id, string CardCode, bool IsReversed, DateTimeOffset CreatedAt);
