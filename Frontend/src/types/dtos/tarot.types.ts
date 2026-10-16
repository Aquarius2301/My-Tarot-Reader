import type { TarotCardCode } from "@/constants";

export interface CardData {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

export interface GetLastDrawnCardForGuestResult extends CardData {
  remainingSeconds: number;
}

export interface CreateDrawForGuestRequest extends CardData {}

export interface GetLastDrawnCardForAuthResult extends CardData {}

export interface CreateDrawForAuthRequest extends CardData {}

export interface GetAllReadingItem extends CardData {
  id: string;
  createdAt: string;
}
export interface GetAllReadingResult {
  items: GetAllReadingItem[];
}
