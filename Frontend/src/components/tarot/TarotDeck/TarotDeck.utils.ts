import { useEffect, useRef, useState } from "react";
import { TAROT_CARDS } from "@/constants";
import { shuffle } from "@/utils";
import { CARD_ASPECT, CARD_SIZE_WIDTH, type TarotCardSize } from "../TarotCard.utils";
import type { SpreadCardState } from "./types";

// ---- Animation timing / sizing, shared between the hook and the component ----

/** Delay between consecutive cards when dealing the spread. */
export const DEAL_STAGGER_MS = 12;
/** Duration of a single card's "slide left" transition. */
export const DEAL_DURATION_MS = 450;
/** Duration of the collect-back-to-center animation before a reshuffle. */
export const COLLECT_DURATION_MS = DEAL_DURATION_MS;
/** How far (px) a selected card is lifted. */
export const RAISE_SELECTED_PX = 28;
/** How far (px) a hovered, clickable card is lifted. */
export const RAISE_HOVER_PX = 14;
/** Fraction of a card's width overlapped by its neighbour on mobile. */
export const MOBILE_OVERLAP_RATIO = 0.42;

export function cardSizeToPx(size: TarotCardSize) {
  const width = CARD_SIZE_WIDTH[size];
  return { width, height: Math.round(width * CARD_ASPECT) };
}

export interface FanLayoutOptions {
  total: number;
  cardWidth: number;
  containerWidth: number;
  isMobile: boolean;
}

export interface FanLayoutResult {
  overlapStep: number;
  spreadWidth: number;
}

/**
 * Computes the horizontal overlap step between adjacent cards.
 * - Web: fit exactly to the container width (no horizontal scroll).
 * - Mobile: fixed step based on a fraction of the card width; the container
 *   scrolls horizontally instead.
 */
export function computeFanLayout({
  total,
  cardWidth,
  containerWidth,
  isMobile,
}: FanLayoutOptions): FanLayoutResult {
  if (total <= 1) return { overlapStep: 0, spreadWidth: cardWidth };

  if (isMobile) {
    const overlapStep = cardWidth * MOBILE_OVERLAP_RATIO;
    return { overlapStep, spreadWidth: cardWidth + overlapStep * (total - 1) };
  }

  const maxStep = cardWidth * 0.85; // avoid over-dense stacking on very wide screens
  const fitStep = (containerWidth - cardWidth) / (total - 1);
  const overlapStep = Math.max(4, Math.min(fitStep, maxStep));
  return { overlapStep, spreadWidth: cardWidth + overlapStep * (total - 1) };
}

/** Tracks the live width of a DOM element (used for responsive layout). */
export function useElementWidth<T extends HTMLElement>() {
  const ref = useRef<T | null>(null);
  const [width, setWidth] = useState(0);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const observer = new ResizeObserver((entries) => {
      const entry = entries[0];
      if (entry) setWidth(entry.contentRect.width);
    });
    observer.observe(el);
    setWidth(el.getBoundingClientRect().width);
    return () => observer.disconnect();
  }, []);

  return { ref, width };
}

/** Builds a freshly shuffled deck with a random upright/reversed orientation. */
export function buildDeck(): SpreadCardState[] {
  return shuffle(TAROT_CARDS).map((code) => ({
    code,
    isReversed: Math.random() < 0.5,
  }));
}
