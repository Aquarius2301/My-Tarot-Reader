/**
 * Spread sizes supported for AI tarot readings. Mirrors the backend `CardCount` enum values.
 */
export const AI_TAROT_CARD_COUNTS = [3, 5, 7, 10] as const;

/** The type representing a valid AI tarot spread size. */
export type AiTarotCardCount = (typeof AI_TAROT_CARD_COUNTS)[number];

/**
 * Spread sizes as returned by the backend, which serializes the `CardCount`
 * enum to camelCase strings ("three" | "five" | "seven" | "ten").
 */
export const AI_TAROT_CARD_COUNT_VALUES = [
  "three",
  "five",
  "seven",
  "ten",
] as const;

/** The type representing a backend card-count enum value. */
export type AiTarotCardCountValue = (typeof AI_TAROT_CARD_COUNT_VALUES)[number];

/** Maps backend card-count enum values to the numeric spread size. */
export const AI_TAROT_CARD_COUNT_BY_VALUE: Record<
  AiTarotCardCountValue,
  AiTarotCardCount
> = {
  three: 3,
  five: 5,
  seven: 7,
  ten: 10,
};

/**
 * Question types for AI tarot readings. Mirrors the backend `QuestionType` enum
 * serialized with camelCase naming (JsonStringEnumConverter).
 */
export const AI_TAROT_QUESTION_TYPES = [
  "energy",
  "love",
  "career",
  "money",
] as const;

/** The type representing a valid AI tarot question type. */
export type AiTarotQuestionType = (typeof AI_TAROT_QUESTION_TYPES)[number];

/**
 * White coin cost of an AI reading, keyed by spread size.
 * Mirrors the backend `AiTarotSetting.Costs` (there is no public cost endpoint).
 */
export const AI_TAROT_COSTS: Record<AiTarotCardCount, number> = {
  3: 2,
  5: 3,
  7: 4,
  10: 5,
};

/**
 * Position keys per spread size, in drawn order. Mirrors the position rules in
 * the backend prompt (`GetPositionRule`). Localized labels live in i18n under
 * `page.aiTarot.position.<key>`.
 */
export const AI_TAROT_POSITIONS: Record<
  AiTarotCardCount,
  readonly string[]
> = {
  3: ["coreEnergy", "challenges", "outcome"],
  5: ["coreEnergy", "challenges", "yourStrength", "future", "outcome"],
  7: [
    "coreEnergy",
    "challenges",
    "yourStrength",
    "hiddenInfluences",
    "wayToFace",
    "future",
    "outcome",
  ],
  10: [
    "coreEnergy",
    "challenges",
    "focus",
    "past",
    "yourStrength",
    "nearFuture",
    "approach",
    "needToKnow",
    "hopesFears",
    "outcome",
  ],
};