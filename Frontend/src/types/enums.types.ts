/**
 * Enum for user roles in the application.
 */
export const USER_ROLE = ["registered", "pro"] as const;

export type UserRole = (typeof USER_ROLE)[number];

/**
 * Lifecycle of a PayOS payment order (backend `PaymentOrderStatus`,
 * serialized lower-cased).
 */
export const PAYMENT_ORDER_STATUS = [
  "pending",
  "paid",
  "cancelled",
  "expired",
  "failed",
] as const;

export type PaymentOrderStatus = (typeof PAYMENT_ORDER_STATUS)[number];
