import { useCallback, useEffect, useRef, useState } from "react";
import {
  COLLECT_DURATION_MS,
  DEAL_DURATION_MS,
  DEAL_STAGGER_MS,
  MOBILE_OVERLAP_RATIO,
} from "./constants";
import type { SpreadCardState, SpreadResultItem } from "./types";
import { TAROT_CARDS } from "@/constants";
import { CARD_ASPECT, CARD_SIZE_WIDTH, type TarotCardSize } from "../TarotCard";

interface FanLayoutOptions {
  total: number;
  cardWidth: number;
  containerWidth: number;
  isMobile: boolean;
}

interface FanLayoutResult {
  overlapStep: number;
  spreadWidth: number;
}

// Fisher-Yates shuffle returning a new array (does not mutate the input).
export function shuffle<T>(arr: readonly T[]): T[] {
  const out = arr.slice();
  for (let i = out.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [out[i], out[j]] = [out[j], out[i]];
  }
  return out;
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

/**
 * Manages the state of a deck that is currently spread out: randomized order and
 * upright/reversed orientation, selection / deselection, reshuffling, and
 * assembling the result when the user confirms.
 */
export function useTarotSpread(limit: number) {
  const [deck, setDeck] = useState<SpreadCardState[]>(() => buildDeck());
  const [dealt, setDealt] = useState(false); // false = collapsed at center, true = spread out
  const [busy, setBusy] = useState(true); // true while an animation runs; locks interaction
  const selectionCounter = useRef(0);
  const timers = useRef<ReturnType<typeof setTimeout>[]>([]);

  const clearTimers = useCallback(() => {
    timers.current.forEach(clearTimeout);
    timers.current = [];
  }, []);

  // Deal the cards for the first time after mount.
  useEffect(() => {
    const raf = requestAnimationFrame(() => setDealt(true));
    const t = setTimeout(
      () => setBusy(false),
      deck.length * DEAL_STAGGER_MS + DEAL_DURATION_MS,
    );
    timers.current.push(t);
    return () => {
      cancelAnimationFrame(raf);
      clearTimers();
    };
    // run exactly once on mount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => clearTimers, [clearTimers]);

  const selectedCount = deck.filter((c) => c.selectedAt !== undefined).length;
  const isFull = selectedCount >= limit;

  const toggleSelect = useCallback(
    (index: number) => {
      if (busy) return;
      setDeck((prev) => {
        const card = prev[index];
        if (!card) return prev;

        if (card.selectedAt !== undefined) {
          // Already selected -> always allow deselection.
          const next = [...prev];
          next[index] = { ...card, selectedAt: undefined };
          return next;
        }

        const currentCount = prev.filter(
          (c) => c.selectedAt !== undefined,
        ).length;
        if (currentCount >= limit) return prev; // at limit -> ignore

        selectionCounter.current += 1;
        const next = [...prev];
        next[index] = { ...card, selectedAt: selectionCounter.current };
        return next;
      });
    },
    [busy, limit],
  );

  const reshuffle = useCallback(() => {
    if (busy) return;
    clearTimers();
    setBusy(true);
    setDealt(false); // collect cards back to center

    const t1 = setTimeout(() => {
      const nextDeck = buildDeck();
      selectionCounter.current = 0; // reset all selections
      setDeck(nextDeck);
      requestAnimationFrame(() => setDealt(true));
      const t2 = setTimeout(
        () => setBusy(false),
        nextDeck.length * DEAL_STAGGER_MS + DEAL_DURATION_MS,
      );
      timers.current.push(t2);
    }, COLLECT_DURATION_MS);
    timers.current.push(t1);
  }, [busy, clearTimers]);

  const confirm = useCallback((): SpreadResultItem[] | null => {
    if (selectedCount !== limit) return null;
    return [...deck]
      .filter((c) => c.selectedAt !== undefined)
      .sort((a, b) => a.selectedAt! - b.selectedAt!)
      .map((c) => ({ cardCode: c.code, isReversed: c.isReversed }));
  }, [deck, limit, selectedCount]);

  return {
    deck,
    dealt,
    busy,
    selectedCount,
    isFull,
    toggleSelect,
    reshuffle,
    confirm,
  };
}

export function cardSizeToPx(size: TarotCardSize) {
  const width = CARD_SIZE_WIDTH[size];
  return { width, height: Math.round(width * CARD_ASPECT) };
}
