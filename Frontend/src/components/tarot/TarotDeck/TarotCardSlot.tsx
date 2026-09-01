import { TarotCard } from "../TarotCard";
import type { TarotCardSize } from "../TarotCard.utils";
import type { SpreadCardState } from "./types";

export interface TarotCardSlotProps {
  card: SpreadCardState;
  index: number;
  dealt: boolean;
  clickable: boolean;
  isSelected: boolean;
  isHovered: boolean;
  targetLeft: number;
  onHover: (index: number | null) => void;
  onToggle: (index: number) => void;
  cardSize: TarotCardSize;
  width: number;
  height: number;
  staggerMs: number;
  raiseSelectedPx: number;
  raiseHoverPx: number;
}

/**
 * A single, absolutely-positioned card slot inside the fan spread. The card is
 * face-down; clicking toggles selection (which lifts it). Hovering lifts a
 * clickable card slightly. All motion is pure CSS transition.
 */
export default function TarotCardSlot({
  card,
  index,
  dealt,
  clickable,
  isSelected,
  isHovered,
  targetLeft,
  onHover,
  onToggle,
  cardSize,
  width,
  height,
  staggerMs,
  raiseSelectedPx,
  raiseHoverPx,
}: TarotCardSlotProps) {
  // Selected cards lift fully; a hovered, still-clickable card lifts a bit.
  const raise = isSelected
    ? raiseSelectedPx
    : isHovered && clickable
      ? raiseHoverPx
      : 0;

  return (
    <div
      key={`${card.code}-${index}`}
      onMouseEnter={() => onHover(index)}
      onMouseLeave={() => onHover(null)}
      style={{
        position: "absolute",
        left: targetLeft,
        // Reserve space at the top so the translateY below pulls the card up
        // into it instead of overflowing the container.
        top: raiseSelectedPx,
        width,
        height,
        transform: `translateY(${dealt ? -raise : 0}px)`,
        transition: "left 0.45s ease, transform 0.25s ease",
        transitionDelay: dealt ? `${index * staggerMs}ms` : "0ms",
        zIndex: index, // keep fan order, don't let a raised card cover others
        scrollSnapAlign: "center",
      }}
    >
      <TarotCard
        cardCode={card.code}
        isUpright={!card.isReversed}
        isFlipped={false}
        size={cardSize}
        onClick={clickable ? () => onToggle(index) : undefined}
      />
    </div>
  );
}
