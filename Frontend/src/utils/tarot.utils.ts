import { TAROT_CARDS, type TarotCardCode } from "@/constants";
import type { AiHistoryCard } from "@/types";

// Fisher-Yates shuffle returning a new array (does not mutate the input).
export function shuffle<T>(arr: readonly T[]): T[] {
  const out = arr.slice();
  for (let i = out.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [out[i], out[j]] = [out[j], out[i]];
  }
  return out;
}

/** Maximum number of card faces rendered inline on the left of an AI history row. */
export const AI_HISTORY_MAX_VISIBLE_CARDS = 4;

const VALID_CARD_CODES = new Set<string>(TAROT_CARDS);

/** Narrowing guard: true when `code` is one of the known tarot card codes. */
function isTarotCardCode(code: string): code is TarotCardCode {
  return VALID_CARD_CODES.has(code);
}

/**
 * Parses the raw stored JSON array of cards into a typed list.
 * The backend stores the serialized reading request DTO — property names are
 * PascalCase (`Code`, `IsReversed`), because the JSON is written server-side
 * with the `System.Text.Json` defaults, not through the API's camelCase converter.
 * Entries with an invalid card code are dropped defensively.
 */
export function parseAiHistoryCards(raw: string): AiHistoryCard[] {
  try {
    const parsed = JSON.parse(raw) as unknown;
    if (!Array.isArray(parsed)) return [];
    return parsed.flatMap((card) => {
      const record = card as { Code?: unknown; IsReversed?: unknown };
      if (typeof record.Code !== "string" || !isTarotCardCode(record.Code))
        return [];
      return [
        {
          code: record.Code,
          isReversed: record.IsReversed === true,
        },
      ];
    });
  } catch {
    return [];
  }
}
