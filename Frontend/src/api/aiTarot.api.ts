import type {
  CreateAiTarotReadingRequest,
  CreateAiTarotReadingResponse,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "@/constants";

export const aiTarotApi = {
  createAiTarotReading: (
    payload: CreateAiTarotReadingRequest,
  ): Promise<CreateAiTarotReadingResponse> => {
    return axiosClient.post(`${API_URL.AITAROT.READING}`, payload);
  },
};
