import type { GetTarotStatisticResult } from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const adminApi = {
  getTarotStatistic: (): Promise<GetTarotStatisticResult> =>
    axiosClient.get(API_URL.admin.getTarotStatistic),
};
