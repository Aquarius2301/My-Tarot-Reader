import { useCallback, useEffect, useRef, useState } from "react";
import {
  buildDeck,
  COLLECT_DURATION_MS,
  DEAL_DURATION_MS,
  DEAL_STAGGER_MS,
} from "./TarotDeck.utils";
import type { SpreadCardState, SpreadResultItem } from "./types";

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

        const currentCount = prev.filter((c) => c.selectedAt !== undefined).length;
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
