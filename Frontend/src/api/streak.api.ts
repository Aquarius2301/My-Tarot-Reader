import type { GetStreakResult } from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const streakApi = {
  getStreak: (): Promise<GetStreakResult> =>
    axiosClient.get(API_URL.streak.getStreak),

  checkIn: (): Promise<void> => axiosClient.put(API_URL.streak.checkIn),
};
