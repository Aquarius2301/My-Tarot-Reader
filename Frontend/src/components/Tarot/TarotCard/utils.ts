import type { TarotCardCode } from "@/constants";
import { CARD_IMAGE_MAP } from "./constants";

// On disk minor-arcana files are zero-padded (min-wands-01.webp) while the
// constants are not (min-wands-1) — normalize so the lookup matches.
function toImageFileName(code: TarotCardCode): string {
  const m = /^(min-[a-z]+)-(\d+)$/.exec(code);
  if (m) return `${m[1]}-${m[2].padStart(2, "0")}`;
  return code; // major "maj-00" already matches its asset name
}

// Build the bundled URL for a card's face image given its code.
export function cardImageUrl(code: TarotCardCode): string {
  return CARD_IMAGE_MAP[toImageFileName(code)] ?? "";
}
