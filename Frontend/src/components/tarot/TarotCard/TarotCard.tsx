import { memo, type CSSProperties, type KeyboardEvent } from "react";
import { Spin, theme } from "antd";
import type { TarotCardCode } from "@/constants";
import {
  CARD_SIZE_WIDTH,
  CARD_ASPECT,
  type TarotCardSize,
  cardImageUrl,
} from "../TarotCard.utils";
import CardBackMotif from "../CardBackMotif";

export interface TarotCardProps {
  cardCode?: TarotCardCode; // present when face-up
  isUpright?: boolean; // true = upright, false = reversed
  isFlipped?: boolean; // true = face-up, false = face-down (card-back)
  size?: TarotCardSize; // 'sm' | 'md' | 'lg'
  onClick?: () => void;
  loading?: boolean;
}

export default memo(function TarotCard({
  cardCode,
  isUpright = true,
  isFlipped = true,
  size = "md",
  onClick,
  loading = false,
}: TarotCardProps) {
  const { token } = theme.useToken();
  const width = CARD_SIZE_WIDTH[size];
  const height = Math.round(width * CARD_ASPECT);
  const container: CSSProperties = {
    width,
    height,
    perspective: 1000,
    cursor: onClick ? "pointer" : "default",
    borderRadius: Math.max(6, width * 0.06),
  };

  if (loading) {
    return (
      <div
        style={{
          ...container,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          background: token.colorFillQuaternary,
          border: `1px solid ${token.colorBorderSecondary}`,
        }}
        role="status"
        aria-label="Loading tarot card"
      >
        <Spin size={size === "sm" ? "small" : "medium"} />
      </div>
    );
  }

  const idBase = cardCode ?? "card-back";
  const faceLabel = cardCode ?? "card back";
  const orientation = isFlipped ? (isUpright ? "Xuôi" : "Ngược") : "face-down";
  const ariaLabel = `${faceLabel}${cardCode ? `, ${orientation}` : ""}`;

  const handleKey = (e: KeyboardEvent<HTMLDivElement>) => {
    if (onClick && (e.key === "Enter" || e.key === " ")) {
      e.preventDefault();
      onClick();
    }
  };

  const backInner: CSSProperties = {
    position: "absolute",
    inset: 0,
    backfaceVisibility: "hidden",
    WebkitBackfaceVisibility: "hidden",
    transform: "rotateY(180deg)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    borderRadius: "inherit",
    background: `linear-gradient(160deg, ${token.colorPrimary} 0%, ${token.colorPrimaryActive} 55%, ${token.colorPrimary} 100%)`,
    border: `2px solid ${token.colorWarning}`,
    boxShadow: `inset 0 0 0 3px  ${token.colorPrimary}`,
  };

  // Paint the face via background-image rather than <img>: replaced elements
  // fail to composite inside a preserve-3d + backface-visibility:hidden flip in
  // several browsers, leaving only the alt text visible.
  const faceInner: CSSProperties = {
    position: "absolute",
    inset: 0,
    backfaceVisibility: "hidden",
    WebkitBackfaceVisibility: "hidden",
    borderRadius: "inherit",
    overflow: "hidden",
    backgroundColor: token.colorBgContainer,
    border: `1px solid ${token.colorBorderSecondary}`,
    backgroundImage: cardCode ? `url(${cardImageUrl(cardCode)})` : undefined,
    backgroundSize: "cover",
    backgroundPosition: "center",
    backgroundRepeat: "no-repeat",
  };

  // The flip container carries the rotateY. Applying the reversed rotation here
  // (combined with rotateY) makes the whole face pre-rotated to its final
  // orientation, so a reversed card reveals already upside-down instead of
  // flipping upright first then rotating.
  const flipTransform = isFlipped
    ? isUpright
      ? "rotateY(0deg)"
      : "rotateY(0deg) rotate(180deg)"
    : isUpright
      ? "rotateY(180deg)"
      : "rotateY(180deg) rotate(180deg)";

  return (
    <div
      style={container}
      role={onClick ? "button" : undefined}
      tabIndex={onClick ? 0 : undefined}
      onClick={onClick}
      onKeyDown={handleKey}
      aria-label={ariaLabel}
    >
      <div
        style={{
          position: "relative",
          width: "100%",
          height: "100%",
          transformStyle: "preserve-3d",
          WebkitTransformStyle: "preserve-3d",
          transition: "transform 0.5s",
          transform: flipTransform,
          borderRadius: "inherit",
        }}
      >
        <div style={faceInner} id={`${idBase}-face`} />
        <div style={backInner} id={`${idBase}-back`}>
          <CardBackMotif color={token.colorWarning} />
        </div>
      </div>
    </div>
  );
});
