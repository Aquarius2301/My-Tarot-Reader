export const API_URL = {
  auth: {
    login: "/api/auth/oauth",
    refresh: "/api/auth/refresh",
    logout: "/api/auth/logout",
    getCurrentUser: "/api/auth/me",
  },
  tarot: {
    getLastDrawnCardForGuest: "/api/tarot/guest-draw",
    createDrawForGuest: "/api/tarot/guest-draw",
    getLastDrawnCardForAuth: "/api/tarot/draw",
    createDrawForAuth: "/api/tarot/draw",
    getAllReading: "/api/tarot",
    deleteReading: "/api/tarot",
  },
} as const;
