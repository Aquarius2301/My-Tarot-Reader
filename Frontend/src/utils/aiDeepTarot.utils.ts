import type { AiDeepTarotTopic } from "@/constants";
import type {
  AiDeepReadingCard,
  AiDeepTarotAnswer,
  AiDeepTarotAnswerSection,
} from "@/types";

/**
 * Minimal translation function shape, so this module stays decoupled from
 * i18next while still being able to resolve localized position labels.
 */
type Translate = (key: string) => string;

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

/**
 * Resolves the calendar label (`MM/yyyy`) of one month of the 12 months spread.
 * Month 1 is the month right after `createdAt`, so month 12 is the same
 * calendar month of the following year.
 *
 * Mirrors the backend `DeepTarotConstant.GetMonthLabel`: the month is derived
 * in UTC so the spread stays identical to the one the AI interpreted, no matter
 * which timezone the browser is in. `Date.UTC` rolls the year over on its own
 * when the month index goes past December.
 *
 * @param createdAt the ISO date the reading was created at
 * @param monthNumber the 1-based month of the spread (1..12)
 * @returns the month formatted as `MM/yyyy`
 */
export function getDeepTarotMonthLabel(
  createdAt: string,
  monthNumber: number,
): string {
  const date = new Date(createdAt);
  const shifted = new Date(
    Date.UTC(date.getUTCFullYear(), date.getUTCMonth() + monthNumber, 1),
  );
  const month = String(shifted.getUTCMonth() + 1).padStart(2, "0");
  return `${month}/${shifted.getUTCFullYear()}`;
}

/**
 * Resolves the display label of a spread position, whichever topic it belongs
 * to. The 12 houses labels are static translations; the 12 months labels are
 * calendar values derived from the reading date, so they are not i18n keys.
 *
 * @param topic the topic the reading belongs to
 * @param positionIndex the 0-based index of the position in drawn order
 * @param positionKey the machine key of the position, e.g. `house-3` / `month-3`
 * @param createdAt the ISO date the reading was created at
 * @param t the translation function
 * @returns the position label, or an empty string when the position is unknown
 */
export function getDeepTarotPositionLabel(
  topic: AiDeepTarotTopic,
  positionIndex: number,
  positionKey: string,
  createdAt: string,
  t: Translate,
): string {
  if (!positionKey) return "";

  return topic === "twelveMonths"
    ? getDeepTarotMonthLabel(createdAt, positionIndex + 1)
    : t(`page.aiDeepTarot.spreads.twelveHouses.position.${positionKey}`);
}
