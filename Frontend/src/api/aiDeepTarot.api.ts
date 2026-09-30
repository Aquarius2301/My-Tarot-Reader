import type {
  CreateTwelveHousesReadingRequest,
  CreateTwelveHousesReadingResult,
  CreateTwelveMonthsReadingRequest,
  CreateTwelveMonthsReadingResult,
  GetAiDeepTarotReadingResult,
  GetAllAiDeepTarotReadingResult,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const aiDeepTarotApi = {
  createTwelveHousesReading: (
    request: CreateTwelveHousesReadingRequest,
  ): Promise<CreateTwelveHousesReadingResult> =>
    axiosClient.post(API_URL.aiDeepTarot.createTwelveHouses, request),

  createTwelveMonthsReading: (
    request: CreateTwelveMonthsReadingRequest,
  ): Promise<CreateTwelveMonthsReadingResult> =>
    axiosClient.post(API_URL.aiDeepTarot.createTwelveMonths, request),

  getAiDeepTarotReadingById: (
    readingId: string,
  ): Promise<GetAiDeepTarotReadingResult> =>
    axiosClient.get(`${API_URL.aiDeepTarot.getById}/${readingId}`),

  getAllAiDeepTarotReadings: (): Promise<GetAllAiDeepTarotReadingResult> =>
    axiosClient.get(API_URL.aiDeepTarot.getAll),

  deleteAiDeepTarotReading: (readingId: string): Promise<void> =>
    axiosClient.delete(`${API_URL.aiDeepTarot.delete}/${readingId}`),
};
