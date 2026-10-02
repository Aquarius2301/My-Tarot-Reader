/**
 * Specialized (deep) tarot topics. Mirrors the backend `DeepTarotTopic` enum
 * serialized with camelCase naming (JsonStringEnumConverter). Only topics with
 * a spread definition on the backend are listed here.
 */
export const AI_DEEP_TAROT_TOPICS = [
  "twelveHouses",
  "twelveMonths",
  "crossroads",
] as const;

/** The type representing a supported deep tarot topic. */
export type AiDeepTarotTopic = (typeof AI_DEEP_TAROT_TOPICS)[number];

/**
 * The topics whose spread has a fixed size, so their position keys can be
 * listed statically in `AI_DEEP_TAROT_POSITIONS`.
 */
export type FixedAiDeepTarotTopic = Exclude<AiDeepTarotTopic, "crossroads">;

/**
 * i18n key of the display name of each topic, so pages can label a reading of
 * any topic without knowing the topic tree shape.
 */
export const AI_DEEP_TAROT_TOPIC_LABEL_KEYS: Record<AiDeepTarotTopic, string> =
  {
    twelveHouses: "page.aiDeepTarot.spreads.twelveHouses.title",
    twelveMonths: "page.aiDeepTarot.spreads.twelveMonths.title",
    crossroads: "page.aiDeepTarot.spreads.crossroads.title",
  };

/**
 * The horizon a crossroads decision is evaluated over.
 * Mirrors the backend `CrossroadsTimeFrame` enum serialized with camelCase
 * naming (JsonStringEnumConverter). Ordered from the shortest to the longest.
 */
export const CROSSROADS_TIME_FRAMES = [
  "now",
  "oneToThreeMonths",
  "overSixMonths",
] as const;

/** The type representing a crossroads decision timeframe. */
export type CrossroadsTimeFrame = (typeof CROSSROADS_TIME_FRAMES)[number];

/**
 * The machine key of the closing card of the crossroads spread.
 * Mirrors the backend `DeepTarotConstant.CrossroadsSummaryKey`.
 */
export const CROSSROADS_SUMMARY_KEY = "summary";

/**
 * The fewest / most options a crossroads reading can compare, and the number of
 * cards drawn for each of them. Mirrors the backend `DeepTarotConstant`.
 */
export const CROSSROADS_MIN_OPTIONS = 2;
export const CROSSROADS_MAX_OPTIONS = 4;
export const CROSSROADS_CARDS_PER_OPTION = 3;
export const CROSSROADS_SUMMARY_CARDS = 1;

/**
 * The maximum length of the question and of a single option of a crossroads
 * reading. Mirrors the backend `DeepTarotConstant`.
 */
export const CROSSROADS_QUESTION_MAX_LENGTH = 500;
export const CROSSROADS_OPTION_MAX_LENGTH = 100;

/**
 * Returns the total number of cards the crossroads spread needs for the given
 * number of options: three per option plus one closing card.
 * Mirrors the backend `DeepTarotConstant.GetCrossroadsCardCount`.
 */
export const getCrossroadsCardCount = (optionCount: number): number =>
  optionCount * CROSSROADS_CARDS_PER_OPTION + CROSSROADS_SUMMARY_CARDS;

/**
 * Returns whether the given number of options can be compared by a crossroads
 * reading. Mirrors the backend `DeepTarotConstant.IsValidCrossroadsOptionCount`.
 */
export const isValidCrossroadsOptionCount = (optionCount: number): boolean =>
  optionCount >= CROSSROADS_MIN_OPTIONS && optionCount <= CROSSROADS_MAX_OPTIONS;

/**
 * The aspects each option is read on, in drawn order. Mirrors the backend
 * `DeepTarotConstant.CrossroadsAspects`.
 */
export const CROSSROADS_ASPECTS = ["current", "evolution", "outcome"] as const;

/** The type representing one aspect of a crossroads option. */
export type CrossroadsAspect = (typeof CROSSROADS_ASPECTS)[number];

/**
 * Builds the machine key of one crossroads position.
 * `optionIndex` is 1-based, matching the numbering shown to the user.
 */
export const getCrossroadsPositionKey = (
  optionIndex: number,
  aspect: CrossroadsAspect,
): string => `option-${optionIndex}-${aspect}`;

/**
 * Builds the full list of crossroads position keys, in drawn order: the three
 * aspects of every option, then the closing summary card.
 * Mirrors the backend `DeepTarotConstant.GetCrossroadsPositions`.
 */
export const getCrossroadsPositionKeys = (optionCount: number): string[] => [
  ...Array.from({ length: optionCount }, (_, optionIndex) =>
    CROSSROADS_ASPECTS.map((aspect) =>
      getCrossroadsPositionKey(optionIndex + 1, aspect),
    ),
  ).flat(),
  CROSSROADS_SUMMARY_KEY,
];

/**
 * i18n key of the label of each crossroads aspect. The option name itself is
 * user data, so it is prepended at render time rather than translated here.
 */
export const CROSSROADS_ASPECT_LABEL_KEYS: Record<CrossroadsAspect, string> = {
  current: "page.aiDeepTarot.spreads.crossroads.aspects.current",
  evolution: "page.aiDeepTarot.spreads.crossroads.aspects.evolution",
  outcome: "page.aiDeepTarot.spreads.crossroads.aspects.outcome",
};

/**
 * The number of cards a topic's spread requires.
 * Mirrors the backend `DeepTarotConstant.RequiredCardCounts`.
 * `crossroads` is sized by the number of options, so it is absent here and
 * resolved with `getCrossroadsCardCount` instead.
 */
export const AI_DEEP_TAROT_CARD_COUNTS: Record<FixedAiDeepTarotTopic, number> = {
  twelveHouses: 12,
  twelveMonths: 12,
};

/**
 * Red coin cost of a reading, keyed by topic.
 * Mirrors the backend `DeepTarotConstant.Costs` (there is no public cost endpoint).
 * `crossroads` costs one red coin per option, so it is absent here and resolved
 * with `getCrossroadsCost` instead.
 */
export const AI_DEEP_TAROT_COSTS: Record<FixedAiDeepTarotTopic, number> = {
  twelveHouses: 3,
  twelveMonths: 3,
};

/** Returns the red coin cost of a crossroads reading for the given options. */
export const getCrossroadsCost = (optionCount: number): number => optionCount;

/**
 * Position keys per topic, in drawn order. `cards[i]` is interpreted in the
 * light of `positions[i]`. Mirrors the backend `DeepTarotConstant.*Positions`.
 *
 * The 12 months labels are calendar values (`MM/yyyy`) rather than i18n keys,
 * so they are resolved with `getDeepTarotMonthLabel` instead of a translation.
 * The 12 houses labels are static and live in i18n under
 * `page.aiDeepTarot.spreads.twelveHouses.position.<key>`.
 *
 * `crossroads` is absent on purpose: its spread is sized by the number of
 * options, so its keys are resolved with `getCrossroadsPositionKeys` (or
 * `getDeepTarotPositionKeys`) instead of being listed statically.
 */
export const AI_DEEP_TAROT_POSITIONS: Record<
  FixedAiDeepTarotTopic,
  readonly string[]
> = {
  twelveHouses: [
    "house-1",
    "house-2",
    "house-3",
    "house-4",
    "house-5",
    "house-6",
    "house-7",
    "house-8",
    "house-9",
    "house-10",
    "house-11",
    "house-12",
  ],
  twelveMonths: [
    "month-1",
    "month-2",
    "month-3",
    "month-4",
    "month-5",
    "month-6",
    "month-7",
    "month-8",
    "month-9",
    "month-10",
    "month-11",
    "month-12",
  ],
};
