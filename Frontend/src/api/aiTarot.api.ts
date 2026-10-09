import type {
  CreateAiTarotReadingRequest,
  CreateAiTarotReadingResult,
  GetAllAiTarotReadingResult,
  GetAiTarotReadingResult,
  PageParams,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const aiTarotApi = {
  createAiTarotReading: (
    request: CreateAiTarotReadingRequest,
  ): Promise<CreateAiTarotReadingResult> =>
    axiosClient.post(API_URL.aiTarot.create, request),

  getAiTarotReadingById: (
    readingId: string,
  ): Promise<GetAiTarotReadingResult> =>
    axiosClient.get(`${API_URL.aiTarot.getById}/${readingId}`),

  getAllAiTarotReadings: (params: PageParams): Promise<GetAllAiTarotReadingResult> =>
    axiosClient.get(API_URL.aiTarot.getAll, { params }),

  deleteAiTarotReading: (readingId: string): Promise<void> =>
    axiosClient.delete(`${API_URL.aiTarot.delete}/${readingId}`),
};