/**
 * The user roles supported by the application (already authenticated).
 * These roles define the access level and permissions of users within the application.
 */
export const USER_ROLES = {
  REGISTERED: "registered",
  PRO: "pro",
} as const;

/**
 * The type representing the user roles in the application (already authenticated).
 * It is derived from the `USER_ROLES` constant and can be used for type checking and ensuring that only valid roles are assigned to users.
 */
export type UserRole = (typeof USER_ROLES)[keyof typeof USER_ROLES];
