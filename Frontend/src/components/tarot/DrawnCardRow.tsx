import { theme } from "antd";
import type { TarotCardCode } from "@/constants";
import { TarotCard } from "./TarotCard";
import type { TarotCardSize } from "./TarotCard.utils";
import OrientationTag from "./OrientationTag";

/** A drawn card as consumed by {@link DrawnCardRow}. */
export interface DrawnCard {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

export interface DrawnCardRowProps {
  /** The cards drawn in the reading, in display order. */
  cards: readonly DrawnCard[];
  /** Card face size. Defaults to `sm`. */
  size?: TarotCardSize;
  /** When true, show an {@link OrientationTag} centered under each card. */
  withOrientationTag?: boolean;
  /** Localized label for the orientation tag (required when `withOrientationTag`). */
  orientationTagLabel?: (isReversed: boolean) => string;
}

/**
 * A centered, wrapping row of the cards drawn in a reading, each shown face-up
 * in its actual orientation. Optionally decorates each card with an orientation
 * tag (used in the history modal). Shared by the AI draw/chat result views.
 */
export default function DrawnCardRow({
  cards,
  size = "sm",
  withOrientationTag = false,
  orientationTagLabel,
}: DrawnCardRowProps) {
  const { token } = theme.useToken();

  return (
    <div
      style={{
        display: "flex",
        flexWrap: "wrap",
        justifyContent: "center",
        gap: token.marginSM,
        marginBottom: token.marginLG,
      }}
    >
      {cards.map((card, index) =>
        withOrientationTag ? (
          <div
            key={`${card.cardCode}-${index}`}
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              gap: token.marginXS,
            }}
          >
            <TarotCard
              cardCode={card.cardCode}
              isUpright={!card.isReversed}
              isFlipped
              size={size}
            />
            <OrientationTag isReversed={card.isReversed}>
              {orientationTagLabel?.(card.isReversed)}
            </OrientationTag>
          </div>
        ) : (
          <TarotCard
            key={`${card.cardCode}-${index}`}
            cardCode={card.cardCode}
            isUpright={!card.isReversed}
            isFlipped
            size={size}
          />
        ),
      )}
    </div>
  );
}
