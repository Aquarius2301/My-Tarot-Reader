// auth.hooks.ts
export const AUTH_QUERY_KEY = ["auth"] as const;

// tarot.hooks.ts
export const TAROT_READING_QUERY_KEY = ["tarotReading"] as const;
export const GET_CARD_FOR_GUEST_QUERY_KEY = [
  ...TAROT_READING_QUERY_KEY,
  "getCardForGuest",
] as const;
export const GET_CARD_FOR_AUTH_QUERY_KEY = [
  ...TAROT_READING_QUERY_KEY,
  "getCardForAuth",
] as const;
export const GET_ALL_READING_QUERY_KEY = [
  ...TAROT_READING_QUERY_KEY,
  "getAll",
] as const;

// streak.hooks.ts
export const STREAK_QUERY_KEY = ["streak"] as const;
export const GET_STREAK_QUERY_KEY = [...STREAK_QUERY_KEY, "getStreak"] as const;
