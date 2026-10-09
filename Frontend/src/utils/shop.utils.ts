import type { CreatePaymentResult } from "@/types";

/**
 * Client-side helpers for the shop checkout flow: VND formatting and the
 * sessionStorage cache that keeps the PayOS payload (checkout url + QR) of a
 * pending order alive across reloads, since GET /orders/{id} does not return
 * those two fields.
 */
const CHECKOUT_STORAGE_PREFIX = "my-tarot-reader.shop.checkout.";

export interface CheckoutPayload {
  checkoutUrl: string;
  qrCode: string;
}

export function saveCheckoutPayload(
  orderId: string,
  payload: CheckoutPayload,
): void {
  try {
    sessionStorage.setItem(
      `${CHECKOUT_STORAGE_PREFIX}${orderId}`,
      JSON.stringify(payload),
    );
  } catch {
    // Storage may be unavailable (private mode); the buyer can still pay
    // through the checkout url carried in router state.
  }
}

export function readCheckoutPayload(
  orderId: string,
): CheckoutPayload | null {
  try {
    const raw = sessionStorage.getItem(`${CHECKOUT_STORAGE_PREFIX}${orderId}`);
    return raw === null ? null : (JSON.parse(raw) as CheckoutPayload);
  } catch {
    return null;
  }
}

export function clearCheckoutPayload(orderId: string): void {
  try {
    sessionStorage.removeItem(`${CHECKOUT_STORAGE_PREFIX}${orderId}`);
  } catch {
    // Ignore: nothing to clean up if storage is unavailable.
  }
}

/** Formats an amount as Vietnamese dong, e.g. 27000 -> "27.000 ₫". */
export function formatVnd(amountVnd: number): string {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(amountVnd);
}

const GUID_REGEX =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

/** Checks that a route param looks like the backend order id (a GUID). */
export function isGuid(value: string): boolean {
  return GUID_REGEX.test(value);
}

/** Narrows the router location state to a created order. */
export function readOrderState(
  state: unknown,
  orderId: string,
): CreatePaymentResult | null {
  if (state === null || typeof state !== "object") {
    return null;
  }
  const order = state as CreatePaymentResult;
  return order.orderId === orderId ? order : null;
}
