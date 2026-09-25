import type {
  CreateAiTarotReadingRequest,
  CreateAiTarotReadingResult,
  GetAllAiTarotReadingResult,
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

  getAllAiTarotReadings: (): Promise<GetAllAiTarotReadingResult> =>
    axiosClient.get(API_URL.aiTarot.getAll),

  deleteAiTarotReading: (readingId: string): Promise<void> =>
    axiosClient.delete(`${API_URL.aiTarot.delete}/${readingId}`),
};