import { CARD_COUNT_OPTIONS, type TarotCardCode } from "@/constants";
import type { AiTarotQuestionType } from "./aiTarot.types";

export interface HistoryItem {
  id: string;
  cardCode: TarotCardCode;
  isReversed: boolean;
  createdAt: string;
}

export interface GetHistoryResponse {
  histories: HistoryItem[];
}

/** A single card drawn in an AI read-history record, parsed from the stored JSON string. */
export interface AiHistoryCard {
  /** The card's code (validated against the tarot constants). */
  code: TarotCardCode;
  /** Whether the card was drawn reversed. */
  isReversed: boolean;
}

/** A single AI read-history record returned from the backend. */
export interface AIReadHistoryResult {
  /** The unique identifier of the AI read-history record. */
  id: string;
  /** The number of tarot cards drawn for this reading. */
  cardCount: (typeof CARD_COUNT_OPTIONS)[number];
  /** The category of the user's question. */
  questionType: AiTarotQuestionType;
  /** The AI-generated interpretation text. */
  answer: string;
  /** JSON array string of the drawn cards (PascalCase keys, e.g. `Code`/`IsReversed`). */
  cards: string;
  /** UTC timestamp when the reading was created. */
  createdAt: string;
}
