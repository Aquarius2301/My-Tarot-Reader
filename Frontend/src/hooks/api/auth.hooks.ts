import { authApi } from "@/api";
import { AUTH_QUERY_KEY } from "@/constants";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { LoginRequest } from "@/types";

export const useLogin = () => {
  return useMutation({
    mutationFn: (body: LoginRequest) => authApi.login(body),
  });
};

export const useLogout = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => authApi.logout(),
    onSuccess: () => queryClient.clear(),
  });
};

export const useGetMe = (enabled = true) =>
  useQuery({
    queryKey: AUTH_QUERY_KEY,
    queryFn: authApi.getMe,
    enabled,
    retry: false, // 401 Unauthorized is expected when the user is not logged in, so we don't want to retry.
    staleTime: 5 * 60 * 1000,
  });
