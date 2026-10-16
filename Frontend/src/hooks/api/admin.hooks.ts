import { adminApi } from "@/api";
import { useQuery } from "@tanstack/react-query";
import { GET_TAROT_STATISTIC_QUERY_KEY } from "./queryKey";

export const useGetTarotStatistic = () =>
  useQuery({
    queryKey: GET_TAROT_STATISTIC_QUERY_KEY,
    queryFn: adminApi.getTarotStatistic,
  });
