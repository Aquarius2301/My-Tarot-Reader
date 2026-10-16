import type {
  CreateDrawForAuthRequest,
  CreateDrawForGuestRequest,
  GetAllReadingResult,
  GetLastDrawnCardForAuthResult,
  GetLastDrawnCardForGuestResult,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const tarotReadingApi = {
  getLastDrawnCardForGuest:
    (): Promise<GetLastDrawnCardForGuestResult | null> =>
      axiosClient.get(API_URL.tarot.getLastDrawnCardForGuest),

  createDrawForGuest: (request: CreateDrawForGuestRequest): Promise<void> =>
    axiosClient.post(API_URL.tarot.createDrawForGuest, request),

  getLastDrawnCardForAuth: (): Promise<GetLastDrawnCardForAuthResult | null> =>
    axiosClient.get(API_URL.tarot.getLastDrawnCardForAuth),

  createDrawForAuth: (request: CreateDrawForAuthRequest): Promise<void> =>
    axiosClient.post(API_URL.tarot.createDrawForAuth, request),

  getAllReading: (): Promise<GetAllReadingResult> =>
    axiosClient.get(API_URL.tarot.getAllReading),

  deleteReading: (historyId: string): Promise<void> =>
    axiosClient.delete(`${API_URL.tarot.deleteReading}/${historyId}`),
};
