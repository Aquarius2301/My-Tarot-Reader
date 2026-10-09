import { useEffect, useRef } from "react";
import { shopApi } from "@/api";
import type {
  CreatePaymentRequest,
  CreatePaymentResult,
  GetOrderStatusResult,
} from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  AUTH_QUERY_KEY,
  GET_ORDER_STATUS_QUERY_KEY,
  GET_PACKAGES_QUERY_KEY,
  GET_WALLET_QUERY_KEY,
} from "./queryKey";

export const useGetPackages = (enabled = true) =>
  useQuery({
    queryKey: GET_PACKAGES_QUERY_KEY,
    queryFn: shopApi.getPackages,
    enabled,
  });

export const useCreateOrder = () =>
  useMutation({
    mutationFn: (request: CreatePaymentRequest): Promise<CreatePaymentResult> =>
      shopApi.createOrder(request),
  });

export const useGetOrderStatus = (orderId: string) =>
  useQuery({
    queryKey: [...GET_ORDER_STATUS_QUERY_KEY, orderId],
    queryFn: () => shopApi.getOrderStatus(orderId),
    enabled: orderId.length > 0,
    // The backend reconciles the order with PayOS on every poll, so keep
    // asking until the order settles; continue while the checkout tab has
    // focus elsewhere (the buyer pays in another tab).
    refetchInterval: (query) =>
      query.state.data?.status === "pending" ? 3000 : false,
    refetchIntervalInBackground: true,
    retry: false,
  });

/**
 * Refreshes the header coin badges and the wallet page once an order becomes
 * `paid`, since the backend credits the red coins at that moment.
 */
export const useRefreshBalancesAfterPaid = (
  status: GetOrderStatusResult["status"] | undefined,
) => {
  const queryClient = useQueryClient();
  const refreshed = useRef(false);

  useEffect(() => {
    if (status !== "paid" || refreshed.current) {
      return;
    }
    refreshed.current = true;
    queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
    queryClient.invalidateQueries({ queryKey: GET_WALLET_QUERY_KEY });
  }, [status, queryClient]);
};
