import { streakApi } from "@/api";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AUTH_QUERY_KEY, GET_STREAK_QUERY_KEY } from "./queryKey";

export const useGetStreak = (enabled: boolean = true) =>
  useQuery({
    queryKey: GET_STREAK_QUERY_KEY,
    queryFn: () => streakApi.getStreak(),
    enabled,
    staleTime: Infinity, // Only changes once per day, so we can cache indefinitely.
  });

export const useCheckIn = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => streakApi.checkIn(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_STREAK_QUERY_KEY });
      // Check-in grants white coins shown in the header, so refresh the user cache.
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
    },
  });
};