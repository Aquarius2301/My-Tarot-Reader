import type { TarotCardCode } from "@/constants";

export interface HistoryItem {
  id: string;
  cardCode: TarotCardCode;
  isReversed: boolean;
  createdAt: string;
}

export interface GetHistoryResponse {
  histories: HistoryItem[];
}
