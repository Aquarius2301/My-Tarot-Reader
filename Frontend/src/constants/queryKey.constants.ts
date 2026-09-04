// auth.hooks.ts
export const AUTH_QUERY_KEY = ["auth"] as const;

// history.hooks.ts
export const HISTORY_QUERY_KEY = ["history"] as const;
export const GET_HISTORY_READINGS_KEY = [
  ...HISTORY_QUERY_KEY,
  "getHistoryReadings",
] as const;

// tarot.hooks.ts
export const TAROT_QUERY_KEY = ["tarot"] as const;
export const AVAILABLE_TIME_QUERY_KEY = [
  ...TAROT_QUERY_KEY,
  "availableTimeForGuest",
] as const;
export const LAST_DRAWN_CARD_QUERY_KEY = [
  ...TAROT_QUERY_KEY,
  "lastDrawnCardForAuth",
] as const;
