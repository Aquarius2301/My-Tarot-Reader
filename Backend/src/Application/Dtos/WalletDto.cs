namespace MyTarotReader.Application.Dtos;

// ── Get Wallet ──

/// <summary>
/// Response containing the user's current coin balances.
/// </summary>
/// <param name="WhiteCoin">The user's white coin balance.</param>
/// <param name="RedCoin">The user's red coin balance.</param>
public record GetWalletResponse(int WhiteCoin, int RedCoin);

// ── Add White Coin ──

/// <summary>
/// Response after successfully adding white coins.
/// </summary>
/// <param name="WhiteCoin">The updated white coin balance.</param>
/// <param name="RedCoin">The current red coin balance.</param>
public record AddWhiteCoinResponse(int WhiteCoin, int RedCoin);

// ── Add Red Coin ──

/// <summary>
/// Response after successfully adding red coins.
/// </summary>
/// <param name="WhiteCoin">The current white coin balance.</param>
/// <param name="RedCoin">The updated red coin balance.</param>
public record AddRedCoinResponse(int WhiteCoin, int RedCoin);

// ── Deduct Coin ──

/// <summary>
/// Response after successfully deducting coins.
/// Coins are consumed from WhiteCoin first, then RedCoin for the remainder.
/// </summary>
/// <param name="WhiteCoin">The updated white coin balance after deduction.</param>
/// <param name="RedCoin">The updated red coin balance after deduction.</param>
public record DeductCoinResponse(int WhiteCoin, int RedCoin);
