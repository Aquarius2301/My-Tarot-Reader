import { aiTarotAdminApi } from "@/api";
import type {
  CreateAiTarotReadingCountRequest,
  CreateAiTarotReadingTypeRequest,
  UpdateAiTarotReadingCountRequest,
  UpdateAiTarotReadingTypeRequest,
} from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AI_TAROT_ADMIN_QUERY_KEY,
  GET_ADMIN_AI_TAROT_SETTINGS_QUERY_KEY,
  GET_AI_TAROT_SETTINGS_QUERY_KEY,
} from "./queryKey";

export const useGetAdminAiTarotSettings = () =>
  useQuery({
    queryKey: GET_ADMIN_AI_TAROT_SETTINGS_QUERY_KEY,
    queryFn: aiTarotAdminApi.getSettings,
  });

const useInvalidateAiTarotSettings = () => {
  const queryClient = useQueryClient();

  return () => {
    queryClient.invalidateQueries({ queryKey: AI_TAROT_ADMIN_QUERY_KEY });
    queryClient.invalidateQueries({ queryKey: GET_AI_TAROT_SETTINGS_QUERY_KEY });
  };
};

export const useCreateAiTarotReadingCount = () => {
  const invalidate = useInvalidateAiTarotSettings();

  return useMutation({
    mutationFn: (
      request: CreateAiTarotReadingCountRequest,
    ): Promise<string> => aiTarotAdminApi.createReadingCount(request),
    onSuccess: invalidate,
  });
};

export const useUpdateAiTarotReadingCount = () => {
  const invalidate = useInvalidateAiTarotSettings();

  return useMutation({
    mutationFn: ({
      id,
      request,
    }: {
      id: string;
      request: UpdateAiTarotReadingCountRequest;
    }): Promise<void> => aiTarotAdminApi.updateReadingCount(id, request),
    onSuccess: invalidate,
  });
};

export const useDeleteAiTarotReadingCount = () => {
  const invalidate = useInvalidateAiTarotSettings();

  return useMutation({
    mutationFn: (id: string): Promise<void> =>
      aiTarotAdminApi.deleteReadingCount(id),
    onSuccess: invalidate,
  });
};

export const useCreateAiTarotReadingType = () => {
  const invalidate = useInvalidateAiTarotSettings();

  return useMutation({
    mutationFn: (
      request: CreateAiTarotReadingTypeRequest,
    ): Promise<string> => aiTarotAdminApi.createReadingType(request),
    onSuccess: invalidate,
  });
};

export const useUpdateAiTarotReadingType = () => {
  const invalidate = useInvalidateAiTarotSettings();

  return useMutation({
    mutationFn: ({
      id,
      request,
    }: {
      id: string;
      request: UpdateAiTarotReadingTypeRequest;
    }): Promise<void> => aiTarotAdminApi.updateReadingType(id, request),
    onSuccess: invalidate,
  });
};

export const useDeleteAiTarotReadingType = () => {
  const invalidate = useInvalidateAiTarotSettings();

  return useMutation({
    mutationFn: (id: string): Promise<void> =>
      aiTarotAdminApi.deleteReadingType(id),
    onSuccess: invalidate,
  });
};
