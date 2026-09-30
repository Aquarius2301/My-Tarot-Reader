import type { AiDeepTarotTopic, TarotCardCode } from "@/constants";

/** A single drawn card sent to the backend when creating a deep tarot reading. */
export interface AiDeepCardRequest {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

/**
 * Request payload for creating a 12 astrological houses deep tarot reading.
 * `cards.length` must be exactly 12.
 */
export interface CreateTwelveHousesReadingRequest {
  locale: string;
  cards: AiDeepCardRequest[];
}

/** Response of creating a 12 astrological houses deep tarot reading. */
export interface CreateTwelveHousesReadingResult {
  id: string;
}

/**
 * Request payload for creating a 12 months deep tarot reading.
 * `cards.length` must be exactly 12; card 1 covers the month after the current one.
 */
export interface CreateTwelveMonthsReadingRequest {
  locale: string;
  cards: AiDeepCardRequest[];
}

/** Response of creating a 12 months deep tarot reading. */
export interface CreateTwelveMonthsReadingResult {
  id: string;
}

/** A drawn tarot card of an existing deep tarot reading. */
export interface AiDeepReadingCard {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

/** A single spread position entry inside the AI-generated answer JSON. */
export interface AiDeepTarotAnswerSection {
  key: string;
  title: string;
  cardCode: string;
  interpretation: string;
}

/** The structured AI-generated answer, stored as a JSON string on the reading. */
export interface AiDeepTarotAnswer {
  title: string;
  overview: string;
  sections: AiDeepTarotAnswerSection[];
  overallAdvice: string;
}

/** Result of retrieving a single deep tarot reading. */
export interface GetAiDeepTarotReadingResult {
  id: string;
  topic: AiDeepTarotTopic;
  title: string;
  answer: string;
  cards: AiDeepReadingCard[];
  createdAt: string;
}

/** A single deep tarot reading entry in the user's reading history. */
export interface GetAllAiDeepTarotReadingItem {
  id: string;
  topic: AiDeepTarotTopic;
  title: string;
  answerSummary: string;
  cards: AiDeepReadingCard[];
  createdAt: string;
}

/** Result of retrieving all deep tarot readings for a user. */
export interface GetAllAiDeepTarotReadingResult {
  items: GetAllAiDeepTarotReadingItem[];
}
