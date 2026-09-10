import { aiTarotApi } from "@/api";
import { AUTH_QUERY_KEY, GET_AI_HISTORY_READINGS_KEY } from "@/constants";
import type { CreateAiTarotReadingRequest } from "@/types";
import { useMutation, useQueryClient } from "@tanstack/react-query";

export const useCreateAiTarotReading = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateAiTarotReadingRequest) =>
      aiTarotApi.createAiTarotReading(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_AI_HISTORY_READINGS_KEY });
      // Reading costs coins, so refresh the wallet balances shown in the header.
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
    },
  });
};
