import type { LoginRequest, UserResponse } from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "@/constants";

export const authApi = {
  // Tokens are delivered via HttpOnly cookies; the body returns only the user.
  login: (body: LoginRequest): Promise<void> =>
    axiosClient.post(API_URL.AUTH.LOGIN, body),

  // Relies on the refreshToken HttpOnly cookie; returns the current user.
  refresh: (): Promise<void> => axiosClient.post(API_URL.AUTH.REFRESH),

  // Relies on the refreshToken HttpOnly cookie.
  logout: (): Promise<void> => axiosClient.post(API_URL.AUTH.LOGOUT),

  // Authenticated via the accessToken HttpOnly cookie.
  getMe: (): Promise<UserResponse> => axiosClient.get(API_URL.AUTH.GET_ME),
};
