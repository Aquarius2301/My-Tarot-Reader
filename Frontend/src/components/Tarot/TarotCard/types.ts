import type { TarotCardCode } from "@/constants";

/** Card size selector, used by the card face. */
export type TarotCardSize = "sm" | "md" | "lg";

export interface TarotCardProps {
  cardCode?: TarotCardCode; // present when face-up
  isUpright?: boolean; // true = upright, false = reversed
  isFlipped?: boolean; // true = face-up, false = face-down (card-back)
  size?: TarotCardSize; // 'sm' | 'md' | 'lg'
  onClick?: () => void;
  loading?: boolean;
}
