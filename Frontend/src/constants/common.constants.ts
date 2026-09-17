export const APP_NAME = "My Tarot Reader" as const;

/**
 * Dispatched on `window` when the refresh-token flow fails (session
 * expired/revoked). The SessionExpiredHandler component listens for it and
 * redirects to the guest home client-side.
 */
export const AUTH_SESSION_EXPIRED_EVENT = "auth:session-expired" as const;
