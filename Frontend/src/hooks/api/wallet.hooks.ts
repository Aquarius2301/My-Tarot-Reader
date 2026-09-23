import { walletApi } from "@/api";
import type { ConvertRedToWhiteRequest } from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AUTH_QUERY_KEY, GET_WALLET_QUERY_KEY } from "./queryKey";

export const useGetWallet = (enabled = true) =>
  useQuery({
    queryKey: GET_WALLET_QUERY_KEY,
    queryFn: walletApi.getWallet,
    enabled,
  });

export const useConvertRedToWhite = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: ConvertRedToWhiteRequest) =>
      walletApi.convertRedToWhite(request),
    onSuccess: () => {
      // Converting touches both balances, and the conversion grants a white
      // batch, so refresh the wallet page data and the header coin badges.
      queryClient.invalidateQueries({ queryKey: GET_WALLET_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
    },
  });
};