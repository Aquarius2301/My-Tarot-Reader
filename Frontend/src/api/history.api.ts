import { API_URL } from "@/constants";
import axiosClient from "./config.api";
import type { AIReadHistoryResult, GetHistoryResponse } from "@/types";

export const historyApi = {
  getHistoryReadings: (): Promise<GetHistoryResponse> => {
    return axiosClient.get(`${API_URL.HISTORY.GET_ALL}`);
  },
  deleteHistory: (historyId: string): Promise<void> => {
    return axiosClient.delete(`${API_URL.HISTORY}/${historyId}`);
  },
  getAiHistoryReadings: (): Promise<AIReadHistoryResult[]> => {
    return axiosClient.get(`${API_URL.HISTORY.GET_AI}`);
  },
  deleteAiHistory: (historyId: string): Promise<void> => {
    return axiosClient.delete(`${API_URL.HISTORY.GET_AI}/${historyId}`);
  },
};
