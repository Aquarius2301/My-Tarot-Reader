import { tarotReadingApi } from "@/api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  GET_ALL_READING_QUERY_KEY,
  GET_CARD_FOR_AUTH_QUERY_KEY,
  GET_CARD_FOR_GUEST_QUERY_KEY,
} from "./queryKey";
import type {
  CreateDrawForAuthRequest,
  CreateDrawForGuestRequest,
} from "@/types";

export const useGetLastDrawnCardForGuest = (enabled: boolean = true) => {
  return useQuery({
    queryKey: GET_CARD_FOR_GUEST_QUERY_KEY,
    queryFn: async () => tarotReadingApi.getLastDrawnCardForGuest(),
    enabled,
    staleTime: Infinity, // The result is valid until the next day, so we can cache it indefinitely.
  });
};

export const useCreateDrawForGuest = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (request: CreateDrawForGuestRequest) =>
      tarotReadingApi.createDrawForGuest(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_CARD_FOR_GUEST_QUERY_KEY });
    },
  });
};

export const useGetLastDrawnCardForAuth = (enabled: boolean = true) => {
  return useQuery({
    queryKey: GET_CARD_FOR_AUTH_QUERY_KEY,
    queryFn: async () => tarotReadingApi.getLastDrawnCardForAuth(),
    enabled,
  });
};

export const useCreateDrawForAuth = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (request: CreateDrawForAuthRequest) =>
      tarotReadingApi.createDrawForAuth(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_CARD_FOR_AUTH_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: GET_ALL_READING_QUERY_KEY });
    },
  });
};

export const useGetAllReading = () =>
  useQuery({
    queryKey: GET_ALL_READING_QUERY_KEY,
    queryFn: () => tarotReadingApi.getAllReading(),
  });

export const useDeleteHistory = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => tarotReadingApi.deleteReading(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_ALL_READING_QUERY_KEY });
    },
  });
};
