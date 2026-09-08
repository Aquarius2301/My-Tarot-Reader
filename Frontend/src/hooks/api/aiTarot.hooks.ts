import { aiTarotApi } from "@/api";
import { GET_AI_HISTORY_READINGS_KEY } from "@/constants";
import type { CreateAiTarotReadingRequest } from "@/types";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export const useCreateAiTarotReading = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateAiTarotReadingRequest) =>
      aiTarotApi.createAiTarotReading(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_AI_HISTORY_READINGS_KEY });
    },
  });
};
