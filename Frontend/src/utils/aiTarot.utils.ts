import type {
  AiReadingCard,
  AiTarotAnswer,
  AiTarotAnswerCard,
} from "@/types";

/** Parses the stored answer JSON into a typed structure, or null if invalid. */
export function parseAiTarotAnswer(rawAnswer: string): AiTarotAnswer | null {
  try {
    const parsed = JSON.parse(rawAnswer) as Partial<AiTarotAnswer>;
    if (
      typeof parsed.overview !== "string" ||
      typeof parsed.overallAdvice !== "string" ||
      !Array.isArray(parsed.cards)
    ) {
      return null;
    }
    return parsed as AiTarotAnswer;
  } catch {
    return null;
  }
}

/**
 * Maps the AI answer cards onto the drawn cards (in drawn order). Each drawn
 * card looks up its interpretation by card code (first unused match), falling
 * back to the answer card at the same array index.
 */
export function matchAnswerCards(
  answer: AiTarotAnswer,
  drawnCards: AiReadingCard[],
): (AiTarotAnswerCard | undefined)[] {
  const used = new Set<number>();
  return drawnCards.map((card, index) => {
    const matchedIndex = answer.cards.findIndex((c, i) => {
      if (used.has(i)) return false;
      return c.cardCode === card.cardCode;
    });

    if (matchedIndex !== -1) {
      used.add(matchedIndex);
      return answer.cards[matchedIndex];
    }

    return answer.cards[index];
  });
}