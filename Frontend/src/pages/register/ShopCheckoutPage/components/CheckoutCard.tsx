import type { GetOrderStatusResult } from "@/types";
import type { CheckoutPayload } from "@/utils";
import { CheckoutCardLoading } from "./CheckoutCardLoading";
import { CheckoutFailedResult } from "./CheckoutFailedResult";
import { CheckoutPaidResult } from "./CheckoutPaidResult";
import { PendingCheckoutCard } from "./PendingCheckoutCard";

export interface CheckoutCardProps {
  order?: GetOrderStatusResult;
  payment: CheckoutPayload | null;
  isLoading?: boolean;
}

/**
 * Renders the checkout for one order according to its lifecycle: a skeleton
 * while the order loads, the PayOS QR + summary while pending, then the paid or
 * failed result once it settles.
 */
export function CheckoutCard({
  order,
  payment,
  isLoading = false,
}: CheckoutCardProps) {
  if (isLoading) {
    return <CheckoutCardLoading />;
  }

  if (order === undefined) {
    return null;
  }

  if (order.status === "pending") {
    return <PendingCheckoutCard order={order} payment={payment} />;
  }

  if (order.status === "paid") {
    return <CheckoutPaidResult order={order} />;
  }

  return <CheckoutFailedResult status={order.status} />;
}
