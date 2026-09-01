import type { TarotCardCode } from "@/constants";
import type { TarotCardSize } from "../TarotCard.utils";

/** State of a single card within the currently spread deck. */
export interface SpreadCardState {
  code: TarotCardCode;
  isReversed: boolean;
  /** Order in which the user selected the card; undefined = not selected. */
  selectedAt?: number;
}

/** A single entry in the result emitted when the user confirms a spread. */
export interface SpreadResultItem {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

export interface TarotDeckProps {
  limit: number;
  onConfirm: (result: SpreadResultItem[]) => void;
  cardSize?: TarotCardSize;
}
