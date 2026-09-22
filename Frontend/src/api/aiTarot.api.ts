import type {
  CreateAiTarotReadingRequest,
  CreateAiTarotReadingResult,
  GetAiTarotReadingResult,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const aiTarotApi = {
  createAiTarotReading: (
    request: CreateAiTarotReadingRequest,
  ): Promise<CreateAiTarotReadingResult> =>
    axiosClient.put(API_URL.aiTarot.create, request),

  getAiTarotReadingById: (
    readingId: string,
  ): Promise<GetAiTarotReadingResult> =>
    axiosClient.get(`${API_URL.aiTarot.getById}/${readingId}`),
};