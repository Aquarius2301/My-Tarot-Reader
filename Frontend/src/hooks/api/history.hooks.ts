import { historyApi } from "@/api";
import { GET_HISTORY_READINGS_KEY, LAST_DRAWN_CARD_QUERY_KEY } from "@/constants";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

export const useGetHistoryReadings = () =>
  useQuery({
    queryKey: GET_HISTORY_READINGS_KEY,
    queryFn: historyApi.getHistoryReadings,
  });

export const useDeleteHistoryReading = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (historyId: string) => historyApi.deleteHistory(historyId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: GET_HISTORY_READINGS_KEY });
      queryClient.invalidateQueries({ queryKey: LAST_DRAWN_CARD_QUERY_KEY }); // assume that the last drawn card may be deleted from history, so we need to invalidate this query as well.
    },
  });
};
