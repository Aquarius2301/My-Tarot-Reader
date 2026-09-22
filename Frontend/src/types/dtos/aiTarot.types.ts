import type {
  AiTarotCardCount,
  AiTarotCardCountValue,
  AiTarotQuestionType,
  TarotCardCode,
} from "@/constants";

/** A single drawn card sent to the backend when creating an AI tarot reading. */
export interface AiCardRequest {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

/** Request payload for creating an AI tarot reading. */
export interface CreateAiTarotReadingRequest {
  cardCount: AiTarotCardCount;
  type: AiTarotQuestionType;
  locale: string;
  cards: AiCardRequest[];
}

/** Response of creating an AI tarot reading. */
export interface CreateAiTarotReadingResult {
  id: string;
}

/** A drawn tarot card of an existing AI reading. */
export interface AiReadingCard {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

/** A single card entry inside the AI-generated answer JSON. */
export interface AiTarotAnswerCard {
  cardCode: string;
  position: string;
  interpretation: string;
}

/** The structured AI-generated answer, stored as a JSON string on the reading. */
export interface AiTarotAnswer {
  title: string;
  overview: string;
  cards: AiTarotAnswerCard[];
  overallAdvice: string;
}

/** Result of retrieving a single AI tarot reading. */
export interface GetAiTarotReadingResult {
  id: string;
  cardCount: AiTarotCardCountValue;
  type: AiTarotQuestionType;
  title: string;
  answer: string;
  cards: AiReadingCard[];
  createdAt: string;
}