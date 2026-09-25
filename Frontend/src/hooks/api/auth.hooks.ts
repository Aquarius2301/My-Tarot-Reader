import { authApi } from "@/api";
import type { GoogleLoginRequest } from "@/types";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AUTH_QUERY_KEY, WALLET_QUERY_KEY } from "./queryKey";

export const useLogin = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (body: GoogleLoginRequest) => authApi.googleLogin(body),
    onSuccess: () => {
      // Drop any stale previous-user /me cache before ProtectedRoute mounts.
      queryClient.invalidateQueries({ queryKey: AUTH_QUERY_KEY });
      // A new sign-up grants the first-login white coin bonus.
      queryClient.invalidateQueries({ queryKey: WALLET_QUERY_KEY });
    },
  });
};

export const useLogout = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => authApi.logout(),
    // Only clear auth-scoped cache keys; leave unrelated queries alive.
    onSuccess: () => {
      queryClient.removeQueries({ queryKey: AUTH_QUERY_KEY });
      queryClient.removeQueries({ queryKey: WALLET_QUERY_KEY });
    },
  });
};

export const useGetCurrentUser = (enabled = true) =>
  useQuery({
    queryKey: AUTH_QUERY_KEY,
    queryFn: authApi.getCurrentUser,
    enabled,
    retry: false, // 401 Unauthorized is expected when the user is not logged in, so we don't want to retry.
  });
