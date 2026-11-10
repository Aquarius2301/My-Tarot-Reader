import type { TarotCardSize } from "./types";

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
export const cardImageModules = import.meta.glob("@/assets/cards/*.webp", {
  eager: true,
  query: "?url",
  import: "default",
}) as Record<string, string>;

export const CARD_IMAGE_MAP: Record<string, string> = {};

for (const [path, url] of Object.entries(cardImageModules)) {
  const file = path
    .split("/")
    .pop()!
    .replace(/\.webp$/, ""); // "maj-00" | "min-wands-01"
  CARD_IMAGE_MAP[file] = url;
}
