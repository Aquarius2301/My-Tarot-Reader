import {
  AI_DEEP_TAROT_POSITIONS,
  getCrossroadsPositionKeys,
  type AiDeepTarotTopic,
  type CrossroadsAspect,
  type CrossroadsTimeFrame,
} from "@/constants";
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

/**
 * Parses a crossroads position key of the form `option-{n}-{aspect}`.
 *
 * @returns the 1-based option index and its aspect, or null when the key is
 * not an option position (i.e. it is the closing `summary` card)
 */
export function parseCrossroadsPositionKey(
  positionKey: string,
): { optionIndex: number; aspect: CrossroadsAspect } | null {
  const segments = positionKey.split("-");
  if (segments.length !== 3 || segments[0] !== "option") return null;

  const optionIndex = Number(segments[1]);
  const aspect = segments[2] as CrossroadsAspect;
  if (!Number.isInteger(optionIndex) || optionIndex < 1) return null;

  return { optionIndex, aspect };
}

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
 * Resolves the position keys of a topic's spread, in drawn order. The 12 houses
 * and 12 months spreads have a fixed size and are listed in
 * `AI_DEEP_TAROT_POSITIONS`; the crossroads spread is sized by the number of
 * options, so its keys are derived instead.
 *
 * @param topic the topic of the spread
 * @param optionCount the number of options, only used by the crossroads spread
 * @returns the position keys in drawn order, or an empty list for an unknown topic
 */
export function getDeepTarotPositionKeys(
  topic: AiDeepTarotTopic,
  optionCount = 0,
): readonly string[] {
  if (topic === "crossroads") return getCrossroadsPositionKeys(optionCount);
  return AI_DEEP_TAROT_POSITIONS[topic] ?? [];
}

/**
 * Resolves the display label of a spread position, whichever topic it belongs
 * to. The 12 houses labels are static translations; the 12 months labels are
 * calendar values derived from the reading date, so they are not i18n keys; the
 * crossroads labels combine the user's own option text with a translated aspect
 * name, so they are not i18n keys either.
 *
 * @param topic the topic the reading belongs to
 * @param positionIndex the 0-based index of the position in drawn order
 * @param positionKey the machine key of the position, e.g. `house-3` / `month-3`
 * @param createdAt the ISO date the reading was created at
 * @param t the translation function
 * @param options the compared options, only used by the crossroads spread
 * @returns the position label, or an empty string when the position is unknown
 */
export function getDeepTarotPositionLabel(
  topic: AiDeepTarotTopic,
  positionIndex: number,
  positionKey: string,
  createdAt: string,
  t: Translate,
  options: readonly string[] = [],
): string {
  if (!positionKey) return "";

  if (topic === "twelveMonths") {
    return getDeepTarotMonthLabel(createdAt, positionIndex + 1);
  }

  if (topic === "crossroads") {
    return getCrossroadsPositionLabel(positionKey, t, options);
  }

  return t(`page.aiDeepTarot.spreads.twelveHouses.position.${positionKey}`);
}

/**
 * Resolves the label of one crossroads position: the user's option text followed
 * by the translated aspect name, or the translated summary label for the closing
 * card.
 */
export function getCrossroadsPositionLabel(
  positionKey: string,
  t: Translate,
  options: readonly string[] = [],
): string {
  const parsed = parseCrossroadsPositionKey(positionKey);
  if (!parsed) return t("page.aiDeepTarot.spreads.crossroads.summaryLabel");

  const option = options[parsed.optionIndex - 1] ?? "";
  return `${option} · ${t(
    `page.aiDeepTarot.spreads.crossroads.aspects.${parsed.aspect}`,
  )}`;
}

/** Resolves the i18n key of a crossroads timeframe label. */
export function getCrossroadsTimeFrameLabelKey(
  timeFrame: CrossroadsTimeFrame,
): string {
  return `page.aiDeepTarot.spreads.crossroads.timeFrames.${timeFrame}`;
}
