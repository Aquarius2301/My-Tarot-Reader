import { aiTarotApi } from "@/api";
import type { CreateAiTarotReadingRequest } from "@/types";
import { useMutation } from "@tanstack/react-query";

export const useCreateAiTarotReading = () => {
  return useMutation({
    mutationFn: (payload: CreateAiTarotReadingRequest) =>
      aiTarotApi.createAiTarotReading(payload),
  });
};
