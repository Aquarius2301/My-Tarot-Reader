import { useMemo, useState } from "react";
import { Grid } from "antd";
import { useTarotSpread } from "./useTarotSpread";
import { useElementWidth, cardSizeToPx, computeFanLayout, DEAL_STAGGER_MS, RAISE_SELECTED_PX, RAISE_HOVER_PX } from "./TarotDeck.utils";
import TarotCardSlot from "./TarotCardSlot";
import TarotSpreadControls from "./TarotSpreadControls";
import type { TarotDeckProps } from "./types";

const { useBreakpoint } = Grid;

/** Renders a face-down fan of 78 tarot cards the user can select and reshuffle. */
export default function TarotDeck({ limit, onConfirm, cardSize = "sm" }: TarotDeckProps) {
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const { ref: containerRef, width: containerWidth } = useElementWidth<HTMLDivElement>();
  const [hoveredIndex, setHoveredIndex] = useState<number | null>(null);

  const {
    deck,
    dealt,
    busy,
    selectedCount,
    isFull,
    toggleSelect,
    reshuffle,
    confirm,
  } = useTarotSpread(limit);

  const { width: cardWidth, height: cardHeight } = cardSizeToPx(cardSize);

  const { overlapStep, spreadWidth } = useMemo(
    () =>
      computeFanLayout({
        total: deck.length,
        cardWidth,
        containerWidth: containerWidth || cardWidth,
        isMobile,
      }),
    [deck.length, cardWidth, containerWidth, isMobile],
  );

  const centerLeft = Math.max((containerWidth - cardWidth) / 2, 0);

  const handleConfirm = () => {
    const result = confirm();
    if (result) onConfirm(result);
  };

  return (
    <div>
      <div
        ref={containerRef}
        style={{
          position: "relative",
          // Reserve headroom above for the raised (selected/hovered) cards.
          height: cardHeight + RAISE_SELECTED_PX + 8,
          width: "100%",
          overflowX: isMobile ? "auto" : "hidden",
          overflowY: "hidden",
          scrollSnapType: isMobile ? "x proximity" : undefined,
          pointerEvents: busy ? "none" : "auto",
        }}
      >
        <div
          style={{
            position: "relative",
            height: "100%",
            width: isMobile ? spreadWidth : "100%",
            minWidth: cardWidth,
          }}
        >
          {deck.map((card, index) => {
            const isSelected = card.selectedAt !== undefined;
            const isHovered = hoveredIndex === index;
            const clickable = isSelected || !isFull;

            const targetLeft = dealt ? index * overlapStep : centerLeft;

            return (
              <TarotCardSlot
                key={`${card.code}-${index}`}
                card={card}
                index={index}
                dealt={dealt}
                clickable={clickable}
                isSelected={isSelected}
                isHovered={isHovered}
                targetLeft={targetLeft}
                onHover={setHoveredIndex}
                onToggle={toggleSelect}
                cardSize={cardSize}
                width={cardWidth}
                height={cardHeight}
                staggerMs={DEAL_STAGGER_MS}
                raiseSelectedPx={RAISE_SELECTED_PX}
                raiseHoverPx={RAISE_HOVER_PX}
              />
            );
          })}
        </div>
      </div>

      <TarotSpreadControls
        selectedCount={selectedCount}
        limit={limit}
        busy={busy}
        isFull={isFull}
        onReshuffle={reshuffle}
        onConfirm={handleConfirm}
      />
    </div>
  );
}
