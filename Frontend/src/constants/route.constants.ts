/**
 * The paths for different pages in the web application.
 * HOME is the public landing page (guest), REGISTERED_HOME is the protected
 * personal dashboard for authenticated users.
 */
export const WEB_URL = {
  HOME: "/guest",
  LOGIN: "/login",
  AUTH_HOME: "/",
  DRAW: "/draw",
  DRAW_GUEST: "/guest/draw",
  HISTORY: "/history",
  AI_DRAW: "/draw/ai",
} as const;

/**
 * `The endpoints for different API requests, such as authentication and tarot card drawing.
 */
export const API_URL = {
  AUTH: {
    LOGIN: "api/v1/auth/oauth",
    LOGOUT: "api/v1/auth/logout",
    REFRESH: "api/v1/auth/refresh",
    GET_ME: "api/v1/auth/me",
  },
  TAROT: {
    GUEST_DRAW: "api/v1/tarot/guest-draw",
    AUTH_DRAW: "api/v1/tarot/draw",
  },
  HISTORY: {
    GET_ALL: "api/v1/history",
  },
  AITAROT: {
    READING: "api/v1/ai-tarot/reading",
  },
} as const;
