import type {
  CardCount,
  LanguageMode,
  QuestionType,
  TarotCardCode,
} from "@/constants";

/**
 * The number of cards drawn for an AI reading. Serialized as a camel-case
 * string ("three" | "five" | "seven" | "ten") matching the backend enum.
 */
export type AiTarotCardCount = "three" | "five" | "seven" | "ten";

/**
 * The category of the user's question for an AI reading. Serialized as a
 * camel-case string matching the backend enum.
 */
export type AiTarotQuestionType = "energy" | "love" | "career" | "money";

/** A single tarot card drawn for an AI reading. */
export interface AiTarotCard {
  /** The card's code (validated against the tarot constants). */
  code: TarotCardCode;
  /** Whether the card was drawn reversed. */
  isReversed: boolean;
}

/** Request body for creating an AI tarot reading. */
export interface CreateAiTarotReadingRequest {
  /** How many cards are drawn for the reading. */
  cardCount: CardCount;
  /** The category of the user's question. */
  questionType: QuestionType;
  /** The drawn cards, one entry per card. */
  cards: AiTarotCard[];
  /** The language for the AI response. */
  language: LanguageMode;
}

/** Response returned after a successful AI tarot reading. */
export interface CreateAiTarotReadingResponse {
  /** The AI-generated interpretation text. */
  answer: string;
}
