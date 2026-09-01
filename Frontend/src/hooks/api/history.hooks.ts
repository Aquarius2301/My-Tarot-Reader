import { historyApi } from "@/api";
import { useQuery } from "@tanstack/react-query";

export const HISTORY_QUERY_KEY = ["history"];
export const GET_HISTORY_READINGS_KEY = ["history", "getHistoryReadings"];

export const useGetHistoryReadings = () =>
  useQuery({
    queryKey: GET_HISTORY_READINGS_KEY,
    queryFn: historyApi.getHistoryReadings,
  });
