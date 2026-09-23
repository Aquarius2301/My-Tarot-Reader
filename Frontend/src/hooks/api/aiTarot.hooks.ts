import { aiTarotApi } from "@/api";
import type { CreateAiTarotReadingRequest } from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AUTH_QUERY_KEY,
  AI_TAROT_QUERY_KEY,
  GET_AI_TAROT_BY_ID_QUERY_KEY,
  GET_ALL_AI_TAROT_QUERY_KEY,
  GET_WALLET_QUERY_KEY,
} from "./queryKey";

export const useCreateAiTarotReading = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (request: CreateAiTarotReadingRequest) =>
      aiTarotApi.createAiTarotReading(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: AI_TAROT_QUERY_KEY });
      // Creating a reading deducts white coins, so refresh the user cache and
      // the wallet page (batches list is also affected).
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: GET_WALLET_QUERY_KEY });
    },
  });
};

export const useGetAiTarotReadingById = (readingId: string, enabled = true) =>
  useQuery({
    queryKey: [...GET_AI_TAROT_BY_ID_QUERY_KEY, readingId],
    queryFn: () => aiTarotApi.getAiTarotReadingById(readingId),
    enabled,
    retry: false, // 404 NotFound is expected when the reading does not exist.
  });

export const useGetAllAiTarotReadings = () =>
  useQuery({
    queryKey: GET_ALL_AI_TAROT_QUERY_KEY,
    queryFn: () => aiTarotApi.getAllAiTarotReadings(),
  });

export const useDeleteAiTarotReading = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (readingId: string) =>
      aiTarotApi.deleteAiTarotReading(readingId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_ALL_AI_TAROT_QUERY_KEY });
    },
  });
};