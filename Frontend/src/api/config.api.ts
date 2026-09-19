import { API_URL } from "./url.api";
import axios, {
  type AxiosError,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";
import { getVisitorId } from "@/utils";
import type { ApiErrorResponse, ApiResponse } from "@/types";
import { AUTH_SESSION_EXPIRED_EVENT } from "@/constants";

const BASE_URL = import.meta.env.VITE_API_URL ?? "";

export const axiosClient = axios.create({
  baseURL: BASE_URL,
  // Required so the HttpOnly, SameSite=None; Secure auth cookies (accessToken,
  // refreshToken) are sent on cross-origin credentialed requests (prod) and
  // same-origin via the dev proxy. Without this the cookies are never sent
  // back and the authenticated endpoints (e.g. /me) can't be reached.
  withCredentials: true,
  headers: {
    "Content-Type": "application/json",
  },
});

// ---------- Types ----------
interface QueueItem {
  resolve: (value: InternalAxiosRequestConfig) => void;
  reject: (reason: unknown) => void;
  config: InternalAxiosRequestConfig;
}

// ---------- Constants ----------
const WHITELIST_API = [
  API_URL.auth.login,
  API_URL.auth.refresh,
  API_URL.auth.logout,
] as const;

const isWhitelisted = (url: string): boolean => {
  return WHITELIST_API.some((endpoint) => url.includes(endpoint));
};

// ---------- Queue management ----------
let isRefreshing = false;
let failedQueue: QueueItem[] = [];

const processQueue = (error: AxiosError | null): void => {
  failedQueue.forEach((item) => {
    if (error) {
      item.reject(error);
    } else {
      item.resolve(item.config);
    }
  });
  failedQueue = [];
};

axiosClient.interceptors.request.use(
  async (config) => {
    // Bind requests to the originating device so refresh tokens can be rejected
    // when replayed from a different browser/profile.
    try {
      const visitorId = await getVisitorId();
      config.headers.set("X-Device-Id", visitorId);
    } catch {
      // Fingerprinting is best-effort; continue without the header if it fails.
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  },
);

axiosClient.interceptors.response.use(
  (response: AxiosResponse<ApiResponse<any>>) => {
    return response.data.data; // Unwrap the data from the API
  },
  async (error: AxiosError<ApiErrorResponse>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean;
    };

    if (!error.response) {
      return Promise.reject(error);
    }

    const { status, config } = error.response;
    const requestUrl = config?.url ?? "";

    // 401 Unauthorized
    if (status === 401) {
      // No refresh for whitelisted endpoints (login, refresh, logout, etc)
      if (isWhitelisted(requestUrl)) {
        return Promise.reject(error.response.data);
      }

      // Avoid looping
      if (originalRequest._retry) {
        return Promise.reject(error.response.data);
      }

      // Refreshing -> queue this request
      if (isRefreshing) {
        return new Promise<InternalAxiosRequestConfig>((resolve, reject) => {
          failedQueue.push({
            resolve,
            reject,
            config: originalRequest,
          });
        })
          .then((config) => axiosClient(config))
          .catch((err) => Promise.reject(err));
      }

      // Start refreshing
      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Fetch refresh endpoint
        await axiosClient.post(API_URL.auth.refresh);

        // Refresh successful -> process queue and retry original request
        processQueue(null);
        return axiosClient(originalRequest);
      } catch (refreshError) {
        // Refresh failed -> process queue with error, redirect login
        processQueue(refreshError as AxiosError);
        // A failed refresh is just "not logged in" when the failing request
        // was the current-user probe: dispatching the session-expired event
        // there would clear the active AUTH query and immediately refetch it
        // (removeQueries on an observed query triggers a refetch), creating an
        // infinite /me -> refresh -> event loop for plain guests.
        if (originalRequest.url !== API_URL.auth.getCurrentUser) {
          // Let the app navigate to the guest home client-side (no full page
          // reload). SessionExpiredHandler mounted in the router listens for this.
          window.dispatchEvent(new Event(AUTH_SESSION_EXPIRED_EVENT));
        }
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error.response.data);
  },
);

export default axiosClient;
