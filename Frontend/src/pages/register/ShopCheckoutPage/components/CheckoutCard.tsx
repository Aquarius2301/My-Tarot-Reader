import type { GetOrderStatusResult } from "@/types";
import type { CheckoutPayload } from "@/utils";
import CheckoutResult from "./CheckoutResult";
import PendingCheckoutCard from "./PendingCheckoutCard";

export interface CheckoutCardProps {
  order: GetOrderStatusResult;
  payment: CheckoutPayload | null;
}

/** Renders a payment order according to its status. */
export default function CheckoutCard({ order, payment }: CheckoutCardProps) {
  if (order.status === "pending") {
    return <PendingCheckoutCard order={order} payment={payment} />;
  }

  return <CheckoutResult order={order} />;
}
