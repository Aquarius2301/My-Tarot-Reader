// ------ Tarot Card Constants ------

/**
 * Major arcana cards codes
 */
export const MAJOR_ARCANA = [
  "maj-00",
  "maj-01",
  "maj-02",
  "maj-03",
  "maj-04",
  "maj-05",
  "maj-06",
  "maj-07",
  "maj-08",
  "maj-09",
  "maj-10",
  "maj-11",
  "maj-12",
  "maj-13",
  "maj-14",
  "maj-15",
  "maj-16",
  "maj-17",
  "maj-18",
  "maj-19",
  "maj-20",
  "maj-21",
] as const;

/**
 * Minor arcana cards codes
 */
export const MINOR_ARCANA = [
  `min-wands-1`,
  `min-wands-2`,
  `min-wands-3`,
  `min-wands-4`,
  `min-wands-5`,
  `min-wands-6`,
  `min-wands-7`,
  `min-wands-8`,
  `min-wands-9`,
  `min-wands-10`,
  `min-wands-11`,
  `min-wands-12`,
  `min-wands-13`,
  `min-wands-14`,
  `min-cups-1`,
  `min-cups-2`,
  `min-cups-3`,
  `min-cups-4`,
  `min-cups-5`,
  `min-cups-6`,
  `min-cups-7`,
  `min-cups-8`,
  `min-cups-9`,
  `min-cups-10`,
  `min-cups-11`,
  `min-cups-12`,
  `min-cups-13`,
  `min-cups-14`,
  `min-swords-1`,
  `min-swords-2`,
  `min-swords-3`,
  `min-swords-4`,
  `min-swords-5`,
  `min-swords-6`,
  `min-swords-7`,
  `min-swords-8`,
  `min-swords-9`,
  `min-swords-10`,
  `min-swords-11`,
  `min-swords-12`,
  `min-swords-13`,
  `min-swords-14`,
  `min-pentacles-1`,
  `min-pentacles-2`,
  `min-pentacles-3`,
  `min-pentacles-4`,
  `min-pentacles-5`,
  `min-pentacles-6`,
  `min-pentacles-7`,
  `min-pentacles-8`,
  `min-pentacles-9`,
  `min-pentacles-10`,
  `min-pentacles-11`,
  `min-pentacles-12`,
  `min-pentacles-13`,
  `min-pentacles-14`,
] as const;

export const TAROT_CARDS = [...MAJOR_ARCANA, ...MINOR_ARCANA] as const;

/**
 * The type representing a major arcana card code, which is one of the predefined major arcana card codes.
 * This type is used to ensure that only valid major arcana card codes are used in the application.
 */
export type MajorArcana = (typeof MAJOR_ARCANA)[number];

/**
 * The type representing a minor arcana card code, which is one of the predefined minor arcana card codes.
 * This type is used to ensure that only valid minor arcana card codes are used in the application.
 */
export type MinorArcana = (typeof MINOR_ARCANA)[number];

/**
 * The type representing a tarot card code, which can be either a major arcana or minor arcana card.
 * This type is used to ensure that only valid tarot card codes are used in the application.
 */
export type TarotCardCode = MajorArcana | MinorArcana;

/**
 * The available sections for tarot card readings, which can be used to categorize and interpret the cards drawn during a reading.
 * This constant defines the supported sections, which can be used for organizing and presenting tarot readings to users.
 */
export const TAROT_SECTIONS = [
  "energy",
  "career",
  "money",
  "love",
  "advice",
] as const;

/**
 * The type representing the sections for tarot card readings, which can be used to categorize and interpret the cards drawn during a reading.
 * It is derived from the `TAROT_SECTIONS` constant and can be used for type checking and ensuring that only valid sections are assigned to tarot readings.
 */
export type TarotSections = (typeof TAROT_SECTIONS)[number];

/**
 * The available question types for tarot card readings, which can be used to categorize and interpret the user's questions during a reading.
 */
export const QUESTION_TYPES = ["energy", "career", "money", "love"] as const;

/**
 * The type representing the question types for tarot card readings, which can be used to categorize and interpret the user's questions during a reading.
 */
export type QuestionType = (typeof QUESTION_TYPES)[number];

/**
 * The available card counts for tarot card readings, which can be used to determine the number of cards drawn during a reading.
 */
export const CARD_COUNT_OPTIONS = ["three", "five", "seven", "ten"] as const;

/**
 * The type representing the card counts for tarot card readings, which can be used to determine the number of cards drawn during a reading.
 */
export type CardCount = (typeof CARD_COUNT_OPTIONS)[number];

/** Numeric limit of cards drawn per card count (drives the TarotDeck limit). */
export const CARD_COUNT_LIMIT: Record<CardCount, number> = {
  three: 3,
  five: 5,
  seven: 7,
  ten: 10,
};
