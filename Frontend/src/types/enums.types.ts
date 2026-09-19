/**
 * Enum for user roles in the application.
 */
export const USER_ROLE = ["registered", "pro"] as const;

export type UserRole = (typeof USER_ROLE)[number];
