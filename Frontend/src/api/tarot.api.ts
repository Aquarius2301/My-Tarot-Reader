import type {
  CreateDrawForAuthRequest,
  GetAvailableTimeResponse,
  GetLastDrawnCardForAuthResponse,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "@/constants";

export const tarotApi = {
  getAvailableTimeForGuest: (
    guestKey: string,
  ): Promise<GetAvailableTimeResponse> => {
    return axiosClient.get(
      `${API_URL.TAROT.GUEST_DRAW}?guestKey=${encodeURIComponent(guestKey)}`,
    );
  },

  createDrawForGuest: (card: {
    guestKey: string;
    cardCode: string;
    isReversed: boolean;
  }): Promise<{ remainingSeconds: number }> => {
    return axiosClient.post(`${API_URL.TAROT.GUEST_DRAW}`, card);
  },

  getLastDrawnCardForAuthAsync:
    (): Promise<GetLastDrawnCardForAuthResponse> => {
      return axiosClient.get(`${API_URL.TAROT.AUTH_DRAW}`);
    },

  createDrawForAuthHistory: (card: CreateDrawForAuthRequest): Promise<void> => {
    return axiosClient.post(`${API_URL.TAROT.AUTH_DRAW}`, card);
  },
};
