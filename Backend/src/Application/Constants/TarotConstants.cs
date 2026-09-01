using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Application.Constants;

/// <summary>
/// Contains the canonical list of all 78 valid tarot card codes.
/// This mirrors the list defined in Frontend/src/constants/tarot.constants.ts.
/// </summary>
public static class TarotConstants
{
    /**
    * Contains the canonical list of all 78 valid tarot card codes and their corresponding English names.
    */
    public static readonly IReadOnlyDictionary<string, TarotCard> AllCards = new Dictionary<
        string,
        TarotCard
    >
    {
        // ==========================================
        // MAJOR ARCANA (22 Cards)
        // ==========================================
        ["maj-00"] = new("maj-00", "The Fool"),
        ["maj-01"] = new("maj-01", "The Magician"),
        ["maj-02"] = new("maj-02", "The High Priestess"),
        ["maj-03"] = new("maj-03", "The Empress"),
        ["maj-04"] = new("maj-04", "The Emperor"),
        ["maj-05"] = new("maj-05", "The Hierophant"),
        ["maj-06"] = new("maj-06", "The Lovers"),
        ["maj-07"] = new("maj-07", "The Chariot"),
        ["maj-08"] = new("maj-08", "Strength"),
        ["maj-09"] = new("maj-09", "The Hermit"),
        ["maj-10"] = new("maj-10", "Wheel of Fortune"),
        ["maj-11"] = new("maj-11", "Justice"),
        ["maj-12"] = new("maj-12", "The Hanged Man"),
        ["maj-13"] = new("maj-13", "Death"),
        ["maj-14"] = new("maj-14", "Temperance"),
        ["maj-15"] = new("maj-15", "The Devil"),
        ["maj-16"] = new("maj-16", "The Tower"),
        ["maj-17"] = new("maj-17", "The Star"),
        ["maj-18"] = new("maj-18", "The Moon"),
        ["maj-19"] = new("maj-19", "The Sun"),
        ["maj-20"] = new("maj-20", "Judgement"),
        ["maj-21"] = new("maj-21", "The World"),

        // ==========================================
        // MINOR ARCANA - WANDS (14 Cards)
        // ==========================================
        ["min-wands-1"] = new("min-wands-1", "Ace of Wands"),
        ["min-wands-2"] = new("min-wands-2", "Two of Wands"),
        ["min-wands-3"] = new("min-wands-3", "Three of Wands"),
        ["min-wands-4"] = new("min-wands-4", "Four of Wands"),
        ["min-wands-5"] = new("min-wands-5", "Five of Wands"),
        ["min-wands-6"] = new("min-wands-6", "Six of Wands"),
        ["min-wands-7"] = new("min-wands-7", "Seven of Wands"),
        ["min-wands-8"] = new("min-wands-8", "Eight of Wands"),
        ["min-wands-9"] = new("min-wands-9", "Nine of Wands"),
        ["min-wands-10"] = new("min-wands-10", "Ten of Wands"),
        ["min-wands-11"] = new("min-wands-11", "Page of Wands"),
        ["min-wands-12"] = new("min-wands-12", "Knight of Wands"),
        ["min-wands-13"] = new("min-wands-13", "Queen of Wands"),
        ["min-wands-14"] = new("min-wands-14", "King of Wands"),

        // ==========================================
        // MINOR ARCANA - CUPS (14 Cards)
        // ==========================================
        ["min-cups-1"] = new("min-cups-1", "Ace of Cups"),
        ["min-cups-2"] = new("min-cups-2", "Two of Cups"),
        ["min-cups-3"] = new("min-cups-3", "Three of Cups"),
        ["min-cups-4"] = new("min-cups-4", "Four of Cups"),
        ["min-cups-5"] = new("min-cups-5", "Five of Cups"),
        ["min-cups-6"] = new("min-cups-6", "Six of Cups"),
        ["min-cups-7"] = new("min-cups-7", "Seven of Cups"),
        ["min-cups-8"] = new("min-cups-8", "Eight of Cups"),
        ["min-cups-9"] = new("min-cups-9", "Nine of Cups"),
        ["min-cups-10"] = new("min-cups-10", "Ten of Cups"),
        ["min-cups-11"] = new("min-cups-11", "Page of Cups"),
        ["min-cups-12"] = new("min-cups-12", "Knight of Cups"),
        ["min-cups-13"] = new("min-cups-13", "Queen of Cups"),
        ["min-cups-14"] = new("min-cups-14", "King of Cups"),

        // ==========================================
        // MINOR ARCANA - SWORDS (14 Cards)
        // ==========================================
        ["min-swords-1"] = new("min-swords-1", "Ace of Swords"),
        ["min-swords-2"] = new("min-swords-2", "Two of Swords"),
        ["min-swords-3"] = new("min-swords-3", "Three of Swords"),
        ["min-swords-4"] = new("min-swords-4", "Four of Swords"),
        ["min-swords-5"] = new("min-swords-5", "Five of Swords"),
        ["min-swords-6"] = new("min-swords-6", "Six of Swords"),
        ["min-swords-7"] = new("min-swords-7", "Seven of Swords"),
        ["min-swords-8"] = new("min-swords-8", "Eight of Swords"),
        ["min-swords-9"] = new("min-swords-9", "Nine of Swords"),
        ["min-swords-10"] = new("min-swords-10", "Ten of Swords"),
        ["min-swords-11"] = new("min-swords-11", "Page of Swords"),
        ["min-swords-12"] = new("min-swords-12", "Knight of Swords"),
        ["min-swords-13"] = new("min-swords-13", "Queen of Swords"),
        ["min-swords-14"] = new("min-swords-14", "King of Swords"),

        // ==========================================
        // MINOR ARCANA - PENTACLES (14 Cards)
        // ==========================================
        ["min-pentacles-1"] = new("min-pentacles-1", "Ace of Pentacles"),
        ["min-pentacles-2"] = new("min-pentacles-2", "Two of Pentacles"),
        ["min-pentacles-3"] = new("min-pentacles-3", "Three of Pentacles"),
        ["min-pentacles-4"] = new("min-pentacles-4", "Four of Pentacles"),
        ["min-pentacles-5"] = new("min-pentacles-5", "Five of Pentacles"),
        ["min-pentacles-6"] = new("min-pentacles-6", "Six of Pentacles"),
        ["min-pentacles-7"] = new("min-pentacles-7", "Seven of Pentacles"),
        ["min-pentacles-8"] = new("min-pentacles-8", "Eight of Pentacles"),
        ["min-pentacles-9"] = new("min-pentacles-9", "Nine of Pentacles"),
        ["min-pentacles-10"] = new("min-pentacles-10", "Ten of Pentacles"),
        ["min-pentacles-11"] = new("min-pentacles-11", "Page of Pentacles"),
        ["min-pentacles-12"] = new("min-pentacles-12", "Knight of Pentacles"),
        ["min-pentacles-13"] = new("min-pentacles-13", "Queen of Pentacles"),
        ["min-pentacles-14"] = new("min-pentacles-14", "King of Pentacles"),
    };

    /// <summary>
    /// Validates whether the provided cardCode exists in the canonical list of tarot cards.
    /// Returns true if the cardCode is valid; false otherwise.
    /// </summary>
    public static bool IsValidCardCode(string? cardCode)
    {
        return !string.IsNullOrWhiteSpace(cardCode) && AllCards.ContainsKey(cardCode);
    }

    /// <summary>
    /// Retrieves the English name of the tarot card corresponding to the given cardCode.
    /// Returns null if the cardCode does not exist in the canonical list.
    /// </summary>
    public static string? GetCardName(string cardCode)
    {
        return AllCards.TryGetValue(cardCode, out var card) ? card.Name : null;
    }

    /// <summary>
    /// Retrieves the TarotCard object corresponding to the given cardCode.
    /// Returns null if the cardCode does not exist in the canonical list.
    /// </summary>
    public static TarotCard? GetCard(string cardCode)
    {
        return AllCards.TryGetValue(cardCode, out var card) ? card : null;
    }
}
