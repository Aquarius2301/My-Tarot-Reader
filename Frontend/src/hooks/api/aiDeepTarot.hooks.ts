import { aiDeepTarotApi } from "@/api";
import type { CreateAiDeepTarotReadingRequest } from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AUTH_QUERY_KEY,
  AI_DEEP_TAROT_QUERY_KEY,
  GET_AI_DEEP_TAROT_BY_ID_QUERY_KEY,
  GET_ALL_AI_DEEP_TAROT_QUERY_KEY,
  GET_WALLET_QUERY_KEY,
} from "./queryKey";

export const useCreateAiDeepTarotReading = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (request: CreateAiDeepTarotReadingRequest) =>
      aiDeepTarotApi.createAiDeepTarotReading(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: AI_DEEP_TAROT_QUERY_KEY });
      // Creating a reading deducts red coins, so refresh the user cache and
      // the wallet page.
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: GET_WALLET_QUERY_KEY });
    },
  });
};

export const useGetAiDeepTarotReadingById = (
  readingId: string,
  enabled = true,
) =>
  useQuery({
    queryKey: [...GET_AI_DEEP_TAROT_BY_ID_QUERY_KEY, readingId],
    queryFn: () => aiDeepTarotApi.getAiDeepTarotReadingById(readingId),
    enabled,
    retry: false, // 404 NotFound is expected when the reading does not exist.
  });

export const useGetAllAiDeepTarotReadings = () =>
  useQuery({
    queryKey: GET_ALL_AI_DEEP_TAROT_QUERY_KEY,
    queryFn: () => aiDeepTarotApi.getAllAiDeepTarotReadings(),
  });

export const useDeleteAiDeepTarotReading = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (readingId: string) =>
      aiDeepTarotApi.deleteAiDeepTarotReading(readingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_ALL_AI_DEEP_TAROT_QUERY_KEY });
    },
  });
};
