namespace MyTarotReader.Application.Dtos;

/// <summary>Represents the information of a Tarot card.</summary>
/// <param name="Code">The unique code of the Tarot card.</param>
/// <param name="Name">The English name of the Tarot card.</param>
public record TarotCard(string Code, string Name);

/// <summary>
/// Response DTO for the last drawn card for an authenticated user.
/// </summary>
/// <param name="CardCode">The code of the tarot card that was drawn.</param>
/// <param name="IsReversed">Whether the drawn card was reversed.</param>
public record GetLastDrawnCardForAuthResponse(string CardCode, bool IsReversed);

/// <summary>Request body for a guest's daily tarot draw.</summary>
/// <param name="GuestKey">The guest's key used for the cooldown cookie.</param>
/// <param name="CardCode">The drawn card's code.</param>
/// <param name="IsReversed">Whether the card was drawn reversed.</param>
public record CreateDrawForGuestRequest(string GuestKey, string CardCode, bool IsReversed);

/// <summary>The already-drawn card for a guest plus seconds until reset.</summary>
/// <param name="CardCode">The drawn card's code (empty if none yet).</param>
/// <param name="IsReversed">Whether the drawn card was reversed.</param>
/// <param name="RemainingSeconds">Seconds left until the draw limit resets.</param>
public record GetLastDrawnCardForGuestResponse(
    string CardCode,
    bool IsReversed,
    long RemainingSeconds
);

/// <summary>
/// Request DTO for persisting a read‑history entry (the drawn card).
/// </summary>
/// <param name="CardCode">The drawn card's code.</param>
/// <param name="IsReversed">Whether the card was drawn reversed.</param>
public record CreateDrawForAuthRequest(string CardCode, bool IsReversed);
