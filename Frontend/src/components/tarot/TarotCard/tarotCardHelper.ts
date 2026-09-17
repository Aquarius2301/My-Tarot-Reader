import type { TarotCardCode } from "@/constants";

/** Card size selector, used by the card face. */
export type TarotCardSize = "sm" | "md" | "lg";

/** Card pixel widths by size. */
export const CARD_SIZE_WIDTH: Record<TarotCardSize, number> = {
  sm: 64,
  md: 96,
  lg: 128,
};

/** Width / height ratio of a tarot card image. */
export const CARD_ASPECT = 527 / 300;

// The `@` alias only works at build time (Vite/TS module resolution) and is
// NOT understood by the browser inside an inline `url(...)`. So we eagerly
// glob the real bundled asset URLs at build time and map them by file name.
const cardImageModules = import.meta.glob("@/assets/cards/*.webp", {
  eager: true,
  query: "?url",
  import: "default",
}) as Record<string, string>;

const CARD_IMAGE_MAP: Record<string, string> = {};
for (const [path, url] of Object.entries(cardImageModules)) {
  const file = path
    .split("/")
    .pop()!
    .replace(/\.webp$/, ""); // "maj-00" | "min-wands-01"
  CARD_IMAGE_MAP[file] = url;
}

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
