import type { TarotCardCode } from "@/constants";

export interface GetAvailableTimeResponse {
  cardCode: TarotCardCode;
  isReversed: boolean;
  remainingSeconds: number;
}

export interface CreateDrawForAuthRequest {
  cardCode: string;
  isReversed: boolean;
}

export interface GetLastDrawnCardForAuthResponse {
  cardCode: TarotCardCode;
  isReversed: boolean;
}
