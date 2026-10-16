using System.Text.Json;
using MyTarotReader.Application.Constants.Tarot;

namespace MyTarotReader.Application.Features.AiTarot;

public record AiTarotAnswerJson(
    string Title,
    string Overview,
    List<AiTarotCardJson> Cards,
    string OverallAdvice
);

public record AiTarotCardJson(string CardCode, string Position, string Interpretation);

public record AiReadingCard(string CardCode, bool IsReversed);

public static class AiTarotHelper
{
    private static readonly int AnswerSummaryMaxLength = 300;

    public static string Truncate(string value)
    {
        if (value.Length <= AnswerSummaryMaxLength)
        {
            return value;
        }

        return value[..AnswerSummaryMaxLength];
    }

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static readonly IReadOnlyDictionary<string, string> CardCodesByName =
        BuildCardCodesByName();

    public static IReadOnlyDictionary<string, string> BuildCardCodesByName()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (code, card) in TarotConstant.AllCards)
        {
            map.TryAdd(NormalizeCardName(card.Name), code);
        }

        return map;
    }

    public static List<AiReadingCard> DeserializeCards(string cardsJson)
    {
        try
        {
            var cards = JsonSerializer.Deserialize<List<AiReadingCard>>(cardsJson, JsonOptions);
            return cards ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public static string NormalizeAnswer(
        AiTarotAnswerJson answer,
        IReadOnlyList<string> drawnCardCodes
    )
    {
        NormalizeAnswerCardCodes(answer, drawnCardCodes);
        return JsonSerializer.Serialize(answer, JsonOptions);
    }

    public static AiTarotAnswerJson? TryParseAnswerJson(string rawAnswer)
    {
        try
        {
            return JsonSerializer.Deserialize<AiTarotAnswerJson>(rawAnswer, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static void NormalizeAnswerCardCodes(
        AiTarotAnswerJson answer,
        IReadOnlyList<string> drawnCardCodes
    )
    {
        if (answer.Cards is null || drawnCardCodes.Count == 0)
        {
            return;
        }

        var used = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < answer.Cards.Count; i++)
        {
            var card = answer.Cards[i];
            var resolved = ResolveCardCode(card.CardCode, drawnCardCodes, used, i);
            used.Add(resolved);
            answer.Cards[i] = card with { CardCode = resolved };
        }
    }

    public static string ResolveCardCode(
        string rawCardCode,
        IReadOnlyList<string> drawnCardCodes,
        HashSet<string> used,
        int index
    )
    {
        if (TarotConstant.IsValidCardCode(rawCardCode))
        {
            return rawCardCode;
        }

        var normalizedName = NormalizeCardName(rawCardCode);

        foreach (var drawnCode in drawnCardCodes)
        {
            if (
                !used.Contains(drawnCode)
                && TarotConstant.GetCardName(drawnCode) is { } drawnName
                && NormalizeCardName(drawnName) == normalizedName
            )
            {
                return drawnCode;
            }
        }

        if (CardCodesByName.TryGetValue(normalizedName, out var deckCode))
        {
            return deckCode;
        }

        return index < drawnCardCodes.Count ? drawnCardCodes[index] : rawCardCode;
    }

    public static string NormalizeCardName(string? value) =>
        new string([.. (value ?? "").Where(char.IsLetterOrDigit)]).ToLowerInvariant();
}
