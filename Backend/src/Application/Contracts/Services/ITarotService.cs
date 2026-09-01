using System;
using System.Threading;
using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Application.Contracts.Services;

public interface ITarotService
{
    /// <summary>
    /// Persists a read‑history entry for the authenticated user.
    /// </summary>
    /// <param name="cardCode">The card code that was drawn.</param>
    /// <param name="isReversed">Whether the card was drawn reversed.</param>
    /// <param name="userId">The id of the authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task CreateDrawForAuthAsync(
        string cardCode,
        bool isReversed,
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the last drawn card for an authenticated user.
    /// </summary>
    /// <param name="userId">The id of the authenticated user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing the last drawn card, or null if no card was drawn.</returns>
    Task<GetLastDrawnCardForAuthResponse?> GetLastDrawnCardForAuthAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the card a guest already drew today and the seconds left until the draw resets.
    /// </summary>
    /// <param name="guestKey">The guest's cookie key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The guest draw state and remaining cooldown.</returns>
    Task<GetLastDrawnCardForGuestResponse> GetLastDrawnCardForGuestAsync(
        string guestKey,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Persists a guest's daily draw (idempotency guarded by Redis).
    /// </summary>
    /// <param name="guestKey">The guest's cookie key.</param>
    /// <param name="cardCode">The drawn card's code.</param>
    /// <param name="isReversed">Whether the card was drawn reversed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task CreateDrawForGuestAsync(
        string guestKey,
        string cardCode,
        bool isReversed,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Removes a guest's stored draw, clearing the daily limit. For testing purposes only.
    /// </summary>
    /// <param name="guestKey">The guest's cookie key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    Task RemoveDrawForGuestAsync(string guestKey, CancellationToken cancellationToken = default);
}
