import { useEffect, useMemo } from "react";
import { Spin } from "antd";
import { useTranslation } from "react-i18next";
import { Navigate, useLocation, useParams } from "react-router-dom";
import { AppTitle, ErrorView } from "@/components";
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

/**
 * PayOS checkout page for one order. Keeps polling the order status until it
 * settles (the backend reconciles with PayOS on each poll) and shows the QR +
 * checkout link while the payment is still pending. The order status response
 * is the source of truth; router state and sessionStorage are fallbacks for
 * orders created before the backend started returning the PayOS payload.
 */
export default function ShopCheckoutPage() {
  const { t } = useTranslation();
  const location = useLocation();
  const { orderId = "" } = useParams<{ orderId: string }>();
  const validOrderId = isGuid(orderId) ? orderId : "";

  const stateOrder = useMemo(
    () => readOrderState(location.state, validOrderId),
    [location.state, validOrderId],
  );

  const { data, isLoading, refetch } = useGetOrderStatus(validOrderId);

  // The order API is the source of truth once it responds; router state and the
  // storage cache are only fallbacks for old orders created before the backend
  // started returning the PayOS payload.
  const payment = useMemo<CheckoutPayload | null>(() => {
    const cached = validOrderId === "" ? null : readCheckoutPayload(validOrderId);
    const checkoutUrl =
      data?.checkoutUrl ||
      stateOrder?.checkoutUrl ||
      cached?.checkoutUrl ||
      "";
    const qrCode = data?.qrCode || stateOrder?.qrCode || cached?.qrCode || "";

    return checkoutUrl === "" && qrCode === ""
      ? null
      : { checkoutUrl, qrCode };
  }, [data?.checkoutUrl, data?.qrCode, stateOrder, validOrderId]);
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
    return <ErrorView type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 800, margin: "0 auto" }}>
      {data.status === "pending" && (
        <AppTitle
          title={t("page.shop.checkoutTitle")}
          subtitle={t("page.shop.checkoutSubtitle")}
        />
      )}

      <CheckoutCard order={data} payment={payment} />
    </div>
  );
}
