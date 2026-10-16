namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Request for tarot reading services for guest users.
/// </summary>
/// <param name="CardCode">The card code.</param>
/// <param name="IsReversed">Indicates if the card is reversed.</param>
public record CreateDrawForGuestRequest(string CardCode, bool IsReversed);

/// <summary>
/// Result of retrieving the last drawn tarot card for a guest user.
/// </summary>
/// <param name="CardCode">The card code.</param>
/// <param name="IsReversed">Indicates if the card is reversed.</param>
/// <param name="RemainingSeconds">The remaining time in seconds before the next draw is allowed.</param>
public record GetLastDrawnCardForGuestResult(
    string CardCode,
    bool IsReversed,
    long RemainingSeconds
);

/// <summary>
/// Request for tarot reading services for authenticated users.
/// </summary>
public record CreateDrawForAuthRequest(string CardCode, bool IsReversed);

/// <summary>
/// Result of retrieving the last drawn tarot card for an authenticated user.
/// </summary>
public record GetLastDrawnCardForAuthResult(string CardCode, bool IsReversed);

/// <summary>
/// Represents an individual tarot reading entry for a user.
/// </summary>
public record GetAllReadingItem(
    Guid Id,
    string CardCode,
    bool IsReversed,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Result of retrieving all tarot readings for a user.
/// </summary>
/// <param name="Items"></param>
public record GetAllReadingResult(List<GetAllReadingItem> Items);

public interface ITarotReadingService
{
    /// <summary>
    /// Creates a new tarot card draw for a guest user.
    /// </summary>
    /// <param name="request"><see cref="CreateDrawForGuestRequest"/> containing the guest key, card code, and reversed status.</param>
    /// <exception cref="TooManyRequestsException">Thrown when the user has already drawn a card.</exception>
    /// <remarks>The card code saved in redis instead of the database for guest users. A new card can be drawn every 12 hours.</remarks>
    /// <exception cref="BadRequestException">Thrown when guest key is empty or card code is invalid.</exception>
    /// <exception cref="TooManyRequestsException">Thrown when the user has already drawn a card.</exception>
    Task CreateDrawForGuestAsync(
        CreateDrawForGuestRequest request,
        string guestKey,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the last drawn tarot card for a guest user.
    /// </summary>
    /// <param name="guestKey">The guest key.</param>
    /// <returns><see cref="GetLastDrawnCardForGuestResult"/> containing the card information and remaining cooldown time.</returns>
    /// <remarks>The card code is retrieved from redis instead of the database for guest users.</remarks>
    Task<GetLastDrawnCardForGuestResult?> GetLastDrawnCardForGuestAsync(
        string guestKey,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Creates a new tarot card draw for an authenticated user.
    /// </summary>
    /// <param name="request"><see cref="CreateDrawForAuthRequest"/> containing the card code and reversed status.</param>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <exception cref="BadRequestException">Thrown when the card code is invalid.</exception>
    Task CreateDrawForAuthAsync(
        CreateDrawForAuthRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the last drawn tarot card for an authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <returns><see cref="GetLastDrawnCardForAuthResult"/> containing the card information, or null if no card has been drawn.</returns>
    Task<GetLastDrawnCardForAuthResult?> GetLastDrawnCardForAuthAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes a tarot card draw for a guest user. (For testing purposes only)
    /// </summary>
    /// <param name="guestKey">The guest key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <remarks>This method is intended for testing purposes only and should not be used in production.</remarks>
    Task RemoveDrawForGuestAsync(string guestKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tarot readings for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve the readings.</param>
    /// <returns><see cref="GetAllTarotReadingResult"/> containing the user's tarot reading history.</returns>
    Task<GetAllReadingResult> GetAllReadingAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a specific tarot reading entry for a user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to delete the reading entry.</param>
    /// <param name="readingId">The ID of the reading entry to delete.</param>
    /// <exception cref="NotFoundException">Thrown when the specified reading entry does not exist for the user.</exception>
    Task DeleteReadingAsync(
        Guid userId,
        Guid readingId,
        CancellationToken cancellationToken = default
    );
}
