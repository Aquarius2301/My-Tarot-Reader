import type {
  CreateAiDeepTarotReadingRequest,
  CreateAiDeepTarotReadingResult,
  GetAiDeepTarotReadingResult,
  GetAllAiDeepTarotReadingResult,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const aiDeepTarotApi = {
  createAiDeepTarotReading: (
    request: CreateAiDeepTarotReadingRequest,
  ): Promise<CreateAiDeepTarotReadingResult> =>
    axiosClient.put(API_URL.aiDeepTarot.create, request),

  getAiDeepTarotReadingById: (
    readingId: string,
  ): Promise<GetAiDeepTarotReadingResult> =>
    axiosClient.get(`${API_URL.aiDeepTarot.getById}/${readingId}`),

  getAllAiDeepTarotReadings: (): Promise<GetAllAiDeepTarotReadingResult> =>
    axiosClient.get(API_URL.aiDeepTarot.getAll),

  deleteAiDeepTarotReading: (readingId: string): Promise<void> =>
    axiosClient.delete(`${API_URL.aiDeepTarot.delete}/${readingId}`),
};
