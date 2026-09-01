import { tarotApi } from "@/api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getVisitorId } from "@/utils/fingerprint.utils";

const TAROT_QUERY_KEY = ["tarot"] as const;

const AVAILABLE_TIME_QUERY_KEY = [
  ...TAROT_QUERY_KEY,
  "availableTimeForGuest",
] as const;

const LAST_DRAWN_CARD_QUERY_KEY = [
  ...TAROT_QUERY_KEY,
  "lastDrawnCardForAuth",
] as const;

export const useGetAvailableTimeForGuest = (enabled: boolean = true) => {
  return useQuery({
    queryKey: AVAILABLE_TIME_QUERY_KEY,
    queryFn: async () =>
      tarotApi.getAvailableTimeForGuest(await getVisitorId()),
    enabled,
    staleTime: Infinity, // The result is valid until the next day, so we can cache it indefinitely.
  });
};

export const useCreateDrawForGuest = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (card: { cardCode: string; isReversed: boolean }) =>
      tarotApi.createDrawForGuest({ ...card, guestKey: await getVisitorId() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: AVAILABLE_TIME_QUERY_KEY });
    },
  });
};

export const useGetLastDrawnCardForAuth = () => {
  return useQuery({
    queryKey: LAST_DRAWN_CARD_QUERY_KEY,
    queryFn: () => tarotApi.getLastDrawnCardForAuthAsync(),
  });
};

export const useCreateDrawForAuth = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (card: { cardCode: string; isReversed: boolean }) =>
      tarotApi.createDrawForAuthHistory(card),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: LAST_DRAWN_CARD_QUERY_KEY });
    },
  });
};
