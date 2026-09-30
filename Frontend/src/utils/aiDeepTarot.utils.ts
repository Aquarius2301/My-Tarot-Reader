import type {
  AiDeepReadingCard,
  AiDeepTarotAnswer,
  AiDeepTarotAnswerSection,
} from "@/types";

/** Parses the stored answer JSON into a typed structure, or null if invalid. */
export function parseAiDeepTarotAnswer(
  rawAnswer: string,
): AiDeepTarotAnswer | null {
  try {
    const parsed = JSON.parse(rawAnswer) as Partial<AiDeepTarotAnswer>;
    if (
      typeof parsed.overview !== "string" ||
      typeof parsed.overallAdvice !== "string" ||
      !Array.isArray(parsed.sections)
    ) {
      return null;
    }
    return parsed as AiDeepTarotAnswer;
  } catch {
    return null;
  }
}

/**
 * Maps the AI answer sections onto the drawn cards (in drawn order). Each drawn
 * card first looks up its section by position key, then by card code (first
 * unused match), falling back to the section at the same array index.
 */
export function matchDeepAnswerSections(
  answer: AiDeepTarotAnswer,
  drawnCards: AiDeepReadingCard[],
  positionKeys: readonly string[],
): (AiDeepTarotAnswerSection | undefined)[] {
  const used = new Set<number>();

  const claim = (index: number) => {
    if (index === -1 || used.has(index)) return undefined;
    used.add(index);
    return answer.sections[index];
  };

  return drawnCards.map((card, index) => {
    const byKey = claim(
      positionKeys[index]
        ? answer.sections.findIndex((s) => s.key === positionKeys[index])
        : -1,
    );
    if (byKey) return byKey;

    const byCode = claim(
      answer.sections.findIndex((s) => s.cardCode === card.cardCode),
    );
    if (byCode) return byCode;

    return claim(index);
  });
}
