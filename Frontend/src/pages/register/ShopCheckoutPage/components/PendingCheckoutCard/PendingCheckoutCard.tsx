import { Col, Row, theme } from "antd";
import type { GetOrderStatusResult } from "@/types";
import type { CheckoutPayload } from "@/utils";
import { CheckoutOrderSummaryCard } from "./CheckoutOrderSummaryCard";
import { CheckoutQrCard } from "./CheckoutQrCard";

export interface PendingCheckoutCardProps {
  order: GetOrderStatusResult;
  payment: CheckoutPayload | null;
}

/**
 * Pending checkout layout: the PayOS QR card comes first (also on mobile),
 * followed by the order summary. The tab keeps polling the order status until
 * it settles.
 */
export function PendingCheckoutCard({
  order,
  payment,
}: PendingCheckoutCardProps) {
  const { token } = theme.useToken();

  return (
    <Row gutter={[token.marginMD, token.marginMD]}>
      <Col xs={24} md={11}>
        <CheckoutQrCard payment={payment} />
      </Col>
      <Col xs={24} md={13}>
        <CheckoutOrderSummaryCard order={order} />
      </Col>
    </Row>
  );
}
