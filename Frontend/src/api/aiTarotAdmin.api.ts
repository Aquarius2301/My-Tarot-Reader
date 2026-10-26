import type {
  CreateAiTarotReadingCountRequest,
  CreateAiTarotReadingTypeRequest,
  GetAllAiTarotSettingsForAdminResult,
  UpdateAiTarotReadingCountRequest,
  UpdateAiTarotReadingTypeRequest,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const aiTarotAdminApi = {
  getSettings: (): Promise<GetAllAiTarotSettingsForAdminResult> =>
    axiosClient.get(API_URL.admin.aiTarotSettings),

  createReadingCount: (
    request: CreateAiTarotReadingCountRequest,
  ): Promise<string> =>
    axiosClient.post(API_URL.admin.aiTarotReadingCounts, request),

  updateReadingCount: (
    id: string,
    request: UpdateAiTarotReadingCountRequest,
  ): Promise<void> =>
    axiosClient.put(`${API_URL.admin.aiTarotReadingCounts}/${id}`, request),

  deleteReadingCount: (id: string): Promise<void> =>
    axiosClient.delete(`${API_URL.admin.aiTarotReadingCounts}/${id}`),

  createReadingType: (
    request: CreateAiTarotReadingTypeRequest,
  ): Promise<string> =>
    axiosClient.post(API_URL.admin.aiTarotReadingTypes, request),

  updateReadingType: (
    id: string,
    request: UpdateAiTarotReadingTypeRequest,
  ): Promise<void> =>
    axiosClient.put(`${API_URL.admin.aiTarotReadingTypes}/${id}`, request),

  deleteReadingType: (id: string): Promise<void> =>
    axiosClient.delete(`${API_URL.admin.aiTarotReadingTypes}/${id}`),
};
