import { useEffect, useState } from "react";
import { Spin, Typography } from "antd";
import { useTranslation } from "react-i18next";
import { Navigate, useLocation, useParams } from "react-router-dom";
import { ErrorComponent } from "@/components";
import { useGetOrderStatus, useRefreshBalancesAfterPaid } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import type { CheckoutPayload } from "@/utils";
import {
  clearCheckoutPayload,
  isGuid,
  readCheckoutPayload,
  readOrderState,
} from "@/utils";
import { CheckoutCard } from "./components";

const { Title, Text } = Typography;

/**
 * PayOS checkout page for one order. Keeps polling the order status until it
 * settles (the backend reconciles with PayOS on each poll) and shows the QR +
 * checkout link while the payment is still pending. The PayOS payload comes
 * from router state right after order creation, falling back to sessionStorage
 * so a reload keeps the QR visible.
 */
export default function ShopCheckoutPage() {
  const { t } = useTranslation();
  const location = useLocation();
  const { orderId = "" } = useParams<{ orderId: string }>();
  const validOrderId = isGuid(orderId) ? orderId : "";

  const [payment] = useState<CheckoutPayload | null>(() => {
    const order = readOrderState(location.state, validOrderId);
    if (order !== null) {
      return { checkoutUrl: order.checkoutUrl, qrCode: order.qrCode };
    }
    return validOrderId === "" ? null : readCheckoutPayload(validOrderId);
  });

  const { data, isLoading, refetch, isFetching } =
    useGetOrderStatus(validOrderId);
  useRefreshBalancesAfterPaid(data?.status);

  // The cached PayOS payload is only needed while the order is pending.
  useEffect(() => {
    if (data !== undefined && data.status !== "pending") {
      clearCheckoutPayload(data.id);
    }
  }, [data]);

  if (validOrderId === "") {
    return <Navigate to={WEB_URL.shop} replace />;
  }

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 720, margin: "0 auto" }}>
      {data.status === "pending" && (
        <>
          <Title level={2} style={{ textAlign: "center", marginBottom: 4 }}>
            {t("page.shop.checkoutTitle")}
          </Title>
          <Text
            type="secondary"
            style={{ display: "block", textAlign: "center", marginBottom: 24 }}
          >
            {t("page.shop.checkoutSubtitle")}
          </Text>
        </>
      )}

      <CheckoutCard
        order={data}
        payment={payment}
        refreshing={isFetching}
        onRefresh={refetch}
      />
    </div>
  );
}
