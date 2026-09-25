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
  streak: {
    getStreak: "/api/streak",
    checkIn: "/api/streak/checkin",
  },
  aiTarot: {
    create: "/api/aiTarot",
    getById: "/api/aiTarot",
    getAll: "/api/aiTarot",
    delete: "/api/aiTarot",
  },
  wallet: {
    getWallet: "/api/wallet",
    convertRedToWhite: "/api/wallet/convert",
  },
} as const;
