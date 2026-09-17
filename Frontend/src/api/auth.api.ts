import type { GetCurrentUserResult, GoogleLoginRequest } from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "./url.api";

export const authApi = {
  // Tokens are delivered via HttpOnly cookies; the body returns only the user.
  googleLogin: (body: GoogleLoginRequest): Promise<void> =>
    axiosClient.post(API_URL.auth.login, body),

  // Relies on the refreshToken HttpOnly cookie; returns the current user.
  refresh: (): Promise<void> => axiosClient.post(API_URL.auth.refresh),

  // Relies on the refreshToken HttpOnly cookie.
  logout: (): Promise<void> => axiosClient.post(API_URL.auth.logout),

  // Authenticated via the accessToken HttpOnly cookie.
  getCurrentUser: (): Promise<GetCurrentUserResult> =>
    axiosClient.get(API_URL.auth.getCurrentUser),
};
