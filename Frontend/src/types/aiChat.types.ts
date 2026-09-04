import type { LanguageMode, TarotCardCode } from "@/constants";

/** A single message in the chat conversation. */
export interface AiChatMessage {
  /** The sender role: "user" or "model" (AI). */
  role: "user" | "model";
  /** The message text content. */
  text: string;
}

/** A single position within a spread recommendation. */
export interface SpreadPosition {
  /** The 1-based position number. */
  position: number;
  /** The meaning/name of this position. */
  name: string;
}

/** A structured spread recommendation parsed from the AI response. */
export interface SpreadRecommendation {
  /** The name of the spread (e.g. "Past-Present-Future"). */
  spreadName: string;
  /** Number of cards to draw. */
  cardCount: number;
  /** The list of positions with their meanings. */
  positions: SpreadPosition[];
}

/** Request body for creating a new AI chat session. */
export interface CreateChatSessionRequest {
  /** The user's free-text tarot question. */
  question: string;
  /** The language for the AI response. */
  language: LanguageMode;
}

/** Response returned after creating a new chat session. */
export interface CreateChatSessionResponse {
  /** The session identifier for follow-up messages. */
  historyId: string;
  /** The AI's initial response text. */
  answer: string;
}

/** Request body for sending a message in an ongoing chat session. */
export interface SendChatMessageRequest {
  /** The session identifier. */
  historyId: string;
  /** The user's new message text. */
  message: string;
  /** The language for the AI response. */
  language: LanguageMode;
}

/** Response returned after sending a chat message. */
export interface SendChatMessageResponse {
  /** The AI's response text. */
  answer: string;
  /** Parsed spread recommendation; undefined if AI hasn't proposed one yet. */
  spreadRecommendation?: SpreadRecommendation;
}

/** A single tarot card drawn for a custom reading. */
export interface AiChatCard {
  /** The card's code (validated against the tarot constants). */
  code: TarotCardCode;
  /** Whether the card was drawn reversed. */
  isReversed: boolean;
}

/** Request body for submitting drawn cards for a custom reading. */
export interface SubmitChatReadingRequest {
  /** The session identifier. */
  historyId: string;
  /** The drawn cards with codes and orientations. */
  cards: AiChatCard[];
  /** The language for the AI response. */
  language: LanguageMode;
}

/** Response returned after submitting cards for a custom reading. */
export interface SubmitChatReadingResponse {
  /** The AI-generated interpretation text. */
  answer: string;
}
