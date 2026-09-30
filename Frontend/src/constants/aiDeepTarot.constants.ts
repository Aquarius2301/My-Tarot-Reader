/**
 * Specialized (deep) tarot topics. Mirrors the backend `DeepTarotTopic` enum
 * serialized with camelCase naming (JsonStringEnumConverter). Only topics with
 * a spread definition on the backend are listed here.
 */
export const AI_DEEP_TAROT_TOPICS = ["twelveHouses"] as const;

/** The type representing a supported deep tarot topic. */
export type AiDeepTarotTopic = (typeof AI_DEEP_TAROT_TOPICS)[number];

/**
 * i18n key of the display name of each topic, so pages can label a reading of
 * any topic without knowing the topic tree shape.
 */
export const AI_DEEP_TAROT_TOPIC_LABEL_KEYS: Record<AiDeepTarotTopic, string> =
  {
    twelveHouses: "page.aiDeepTarot.twelveHouses.title",
  };

/**
 * The number of cards a topic's spread requires.
 * Mirrors the backend `DeepTarotConstant.RequiredCardCounts`.
 */
export const AI_DEEP_TAROT_CARD_COUNTS: Record<AiDeepTarotTopic, number> = {
  twelveHouses: 12,
};

/**
 * Red coin cost of a reading, keyed by topic.
 * Mirrors the backend `DeepTarotConstant.Costs` (there is no public cost endpoint).
 */
export const AI_DEEP_TAROT_COSTS: Record<AiDeepTarotTopic, number> = {
  twelveHouses: 3,
};

/**
 * Position keys per topic, in drawn order. `cards[i]` is interpreted in the
 * light of `positions[i]`. Mirrors the backend `DeepTarotConstant.*Positions`;
 * localized labels live in i18n under `page.aiDeepTarot.house.<key>`.
 */
export const AI_DEEP_TAROT_POSITIONS: Record<
  AiDeepTarotTopic,
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
};
