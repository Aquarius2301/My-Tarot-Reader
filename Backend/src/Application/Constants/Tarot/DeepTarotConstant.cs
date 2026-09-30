using System.Globalization;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Constants.Tarot;

/// <summary>
/// A single position of a deep tarot spread.
/// </summary>
/// <param name="Number">The 1-based position index, matching the drawn card order.</param>
/// <param name="Key">The stable machine key of the position (used by the client and stored answer).</param>
/// <param name="Name">The English name of the position, used when prompting the AI.</param>
/// <param name="Keywords">The themes of the position, used when prompting the AI.</param>
public record DeepTarotPosition(int Number, string Key, string Name, string Keywords);

/// <summary>
/// One of the fixed aspects every option of a dynamically sized spread is read on.
/// </summary>
/// <param name="Key">The machine key suffix of the aspect, used to build the position key.</param>
/// <param name="Name">The English name of the aspect, used when prompting the AI.</param>
/// <param name="Keywords">The themes of the aspect, used when prompting the AI.</param>
public record DeepTarotAspect(string Key, string Name, string Keywords);

/// <summary>
/// Contains the spread definitions of the specialized (deep) tarot reading topics.
/// </summary>
/// <remarks>
/// <see cref="DeepTarotTopic.TwelveHouses"/> and <see cref="DeepTarotTopic.TwelveMonths"/>
/// have a fixed size; <see cref="DeepTarotTopic.Crossroads"/> is sized by the number of
/// options the user submits. The remaining topics are declared on the enum but rejected
/// until their spread is added here.
/// </remarks>
public static class DeepTarotConstant
{
    /// <summary>
    /// The fewest options a crossroads reading can compare.
    /// </summary>
    public const int CrossroadsMinOptions = 2;

    /// <summary>
    /// The most options a crossroads reading can compare.
    /// </summary>
    public const int CrossroadsMaxOptions = 4;

    /// <summary>
    /// The number of cards drawn for each option of the crossroads spread.
    /// </summary>
    public const int CrossroadsCardsPerOption = 3;

    /// <summary>
    /// The extra closing card of the crossroads spread, carrying the overall advice.
    /// </summary>
    public const int CrossroadsSummaryCards = 1;

    /// <summary>
    /// The maximum length of the question a crossroads reading accepts.
    /// </summary>
    public const int CrossroadsQuestionMaxLength = 500;

    /// <summary>
    /// The maximum length of a single option a crossroads reading accepts.
    /// </summary>
    public const int CrossroadsOptionMaxLength = 100;

    /// <summary>
    /// The machine key of the closing card of the crossroads spread.
    /// </summary>
    public const string CrossroadsSummaryKey = "summary";

    /**
     * The 12 astrological houses of the "12 houses" spread, in drawn order:
     * one card per house, from the self (house 1) to the subconscious (house 12).
     */
    public static readonly IReadOnlyList<DeepTarotPosition> TwelveHousesPositions =
    [
        new(1, "house-1", "House 1 - Self and identity", "body, appearance, personality, how you start things"),
        new(2, "house-2", "House 2 - Money and personal values", "income, possessions, self-worth, resources"),
        new(3, "house-3", "House 3 - Mind and communication", "thoughts, siblings, learning, everyday messages"),
        new(4, "house-4", "House 4 - Home and family", "roots, home, ancestors, inner foundation"),
        new(5, "house-5", "House 5 - Creativity and joy", "romance, children, play, self-expression"),
        new(6, "house-6", "House 6 - Health and daily routine", "wellbeing, work habits, service, discipline"),
        new(7, "house-7", "House 7 - Partnership and marriage", "one-to-one relationships, contracts, the other"),
        new(8, "house-8", "House 8 - Transformation and intimacy", "shared resources, crisis, sexuality, inheritance"),
        new(9, "house-9", "House 9 - Belief and expansion", "philosophy, travel, higher study, faith"),
        new(10, "house-10", "House 10 - Career and reputation", "vocation, public standing, ambition, authority"),
        new(11, "house-11", "House 11 - Community and shared goals", "friends, networks, hopes, group success"),
        new(12, "house-12", "House 12 - Subconscious and retreat", "the unseen, solitude, hidden matters, release"),
    ];

    /**
     * The 12 consecutive calendar months of the "12 months" spread, in drawn order:
     * one card per month, starting from the month after the reading is created.
     * The calendar label (MM/yyyy) of each month is resolved when the AI prompt is built.
     */
    public static readonly IReadOnlyList<DeepTarotPosition> TwelveMonthsPositions =
    [
        new(1, "month-1", "Month 1", "the first month of the period, opening moves, new beginnings"),
        new(2, "month-2", "Month 2", "early progress, adjustments, settling in"),
        new(3, "month-3", "Month 3", "building momentum, early results, growing confidence"),
        new(4, "month-4", "Month 4", "stabilising, consolidating the first quarter"),
        new(5, "month-5", "Month 5", "expansion, joy, creative and romantic energy"),
        new(6, "month-6", "Month 6", "mid-year balance, daily routine, health and service"),
        new(7, "month-7", "Month 7", "partnership, cooperation, one-to-one commitments"),
        new(8, "month-8", "Month 8", "transformation, shared resources, deep change"),
        new(9, "month-9", "Month 9", "growth, learning, travel, widening horizons"),
        new(10, "month-10", "Month 10", "career peak, public standing, ambition realised"),
        new(11, "month-11", "Month 11", "community, friends, shared goals and support"),
        new(12, "month-12", "Month 12", "closing the cycle, integration, what to carry forward"),
    ];

    /**
     * The three aspects each option of the crossroads spread is read on, in drawn order.
     */
    private static readonly IReadOnlyList<DeepTarotAspect> CrossroadsAspects =
    [
        new("current", "Current energy", "the energy this option carries right now"),
        new("evolution", "How it develops", "how this option unfolds over the given timeframe"),
        new("outcome", "Outcome", "where this option leads if it is chosen"),
    ];

    /// <summary>
    /// Builds the positions of the crossroads spread: three cards per option followed by
    /// one closing summary card.
    /// </summary>
    /// <param name="optionCount">The number of options being compared.</param>
    /// <returns>The positions of the spread, in drawn order.</returns>
    public static IReadOnlyList<DeepTarotPosition> GetCrossroadsPositions(int optionCount)
    {
        var positions = new List<DeepTarotPosition>(optionCount * CrossroadsCardsPerOption + 1);
        var number = 1;

        for (var option = 1; option <= optionCount; option++)
        {
            foreach (var aspect in CrossroadsAspects)
            {
                positions.Add(
                    new(
                        number++,
                        $"option-{option}-{aspect.Key}",
                        $"Option {option} - {aspect.Name}",
                        aspect.Keywords
                    )
                );
            }
        }

        positions.Add(
            new(
                number,
                CrossroadsSummaryKey,
                "Summary and advice",
                "how the options compare overall, which one the cards favour, and the practical next step"
            )
        );

        return positions;
    }

    private static readonly IReadOnlyDictionary<DeepTarotTopic, int> RequiredCardCounts =
        new Dictionary<DeepTarotTopic, int>
        {
            [DeepTarotTopic.TwelveHouses] = 12,
            [DeepTarotTopic.TwelveMonths] = 12,
        };

    /// <summary>
    /// The number of red coins charged for reading each topic's spread.
    /// </summary>
    private static readonly IReadOnlyDictionary<DeepTarotTopic, int> Costs =
        new Dictionary<DeepTarotTopic, int>
        {
            [DeepTarotTopic.TwelveHouses] = 3,
            [DeepTarotTopic.TwelveMonths] = 3,
        };

    /// <summary>
    /// Determines whether the given topic has a spread definition and can be read.
    /// </summary>
    public static bool IsSupported(DeepTarotTopic topic) =>
        topic == DeepTarotTopic.Crossroads || RequiredCardCounts.ContainsKey(topic);

    /// <summary>
    /// Determines whether the given number of options is within the crossroads spread's range.
    /// </summary>
    public static bool IsValidCrossroadsOptionCount(int optionCount) =>
        optionCount is >= CrossroadsMinOptions and <= CrossroadsMaxOptions;

    /// <summary>
    /// Returns the number of cards the given topic's spread requires.
    /// </summary>
    /// <param name="topic">The topic to resolve.</param>
    /// <param name="optionCount">
    /// The number of options, only used by <see cref="DeepTarotTopic.Crossroads"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the topic has no spread definition.</exception>
    public static int GetRequiredCardCount(DeepTarotTopic topic, int optionCount = 0) =>
        topic switch
        {
            DeepTarotTopic.Crossroads => GetCrossroadsCardCount(optionCount),
            _ => RequiredCardCounts.TryGetValue(topic, out var count)
                ? count
                : throw new ArgumentOutOfRangeException(
                    nameof(topic),
                    topic,
                    "The topic does not have a spread definition yet."
                ),
        };

    /// <summary>
    /// Returns the number of cards the crossroads spread requires for the given options.
    /// </summary>
    public static int GetCrossroadsCardCount(int optionCount) =>
        optionCount * CrossroadsCardsPerOption + CrossroadsSummaryCards;

    /// <summary>
    /// Returns the number of red coins charged for reading the given topic's spread.
    /// </summary>
    /// <param name="topic">The topic to resolve.</param>
    /// <param name="optionCount">
    /// The number of options, only used by <see cref="DeepTarotTopic.Crossroads"/> which
    /// charges one red coin per option.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the topic has no spread definition.</exception>
    public static int GetCost(DeepTarotTopic topic, int optionCount = 0) =>
        topic switch
        {
            DeepTarotTopic.Crossroads => optionCount,
            _ => Costs.TryGetValue(topic, out var cost)
                ? cost
                : throw new ArgumentOutOfRangeException(
                    nameof(topic),
                    topic,
                    "The topic does not have a spread definition yet."
                ),
        };

    /// <summary>
    /// Returns the spread positions of the given topic, in drawn order.
    /// </summary>
    /// <param name="topic">The topic to resolve.</param>
    /// <param name="optionCount">
    /// The number of options, only used by <see cref="DeepTarotTopic.Crossroads"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the topic has no spread definition.</exception>
    public static IReadOnlyList<DeepTarotPosition> GetPositions(
        DeepTarotTopic topic,
        int optionCount = 0
    ) =>
        topic switch
        {
            DeepTarotTopic.TwelveHouses => TwelveHousesPositions,
            DeepTarotTopic.TwelveMonths => TwelveMonthsPositions,
            DeepTarotTopic.Crossroads => GetCrossroadsPositions(optionCount),
            _ => throw new ArgumentOutOfRangeException(
                nameof(topic),
                topic,
                "The topic does not have a spread definition yet."
            ),
        };

    /// <summary>
    /// Resolves the calendar label ("MM/yyyy") of the month drawn on the given position of
    /// the 12 months spread. Position 1 is the month right after <paramref name="createdAt"/>,
    /// so position 12 is the same calendar month of the following year.
    /// </summary>
    /// <remarks>
    /// The month is resolved in UTC to keep the spread stable across server deployments.
    /// </remarks>
    public static string GetMonthLabel(DeepTarotPosition position, DateTimeOffset createdAt) =>
        new DateTimeOffset(
            createdAt.Year,
            createdAt.Month,
            1,
            0,
            0,
            0,
            TimeSpan.Zero
        )
            .AddMonths(position.Number)
            .ToString("MM/yyyy", CultureInfo.InvariantCulture);
}
