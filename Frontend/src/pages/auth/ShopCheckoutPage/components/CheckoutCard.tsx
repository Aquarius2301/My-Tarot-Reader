import { useState } from "react";
import {
  Alert,
  Button,
  Card,
  Result,
  Space,
  Typography,
  theme,
} from "antd";
import {
  ArrowLeftOutlined,
  ClockCircleOutlined,
  LinkOutlined,
  QrcodeOutlined,
  ReloadOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import i18n from "@/i18n";
import { WEB_URL } from "@/routes";
import type { GetOrderStatusResult, PaymentOrderStatus } from "@/types";
import { convertISOToDate, formatVnd, type CheckoutPayload } from "@/utils";
import { CoinIcon } from "@/components";

const { Title, Text } = Typography;

export interface CheckoutCardProps {
  order: GetOrderStatusResult;
  payment: CheckoutPayload | null;
  refreshing: boolean;
  onRefresh: () => void;
}

const FAILED_STATUS_KEY: Record<
  Exclude<PaymentOrderStatus, "pending" | "paid">,
  string
> = {
  cancelled: "page.shop.status.cancelled",
  expired: "page.shop.status.expired",
  failed: "page.shop.status.failed",
};

/**
 * Renders a payment order according to its status: a pending state with the
 * PayOS QR + checkout link and a live poll, a success result crediting the
 * red coins, or a failure result with a way back to the shop.
 */
export default function CheckoutCard({
  order,
  payment,
  refreshing,
  onRefresh,
}: CheckoutCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const navigate = useNavigate();
  const [qrBroken, setQrBroken] = useState(false);

  if (order.status === "paid") {
    const paidAt =
      order.paidAt === null
        ? null
        : convertISOToDate(order.paidAt, i18n.language);

    return (
      <Result
        status="success"
        title={t("page.shop.paidTitle")}
        subTitle={t("page.shop.paidSubtitle", { redCoins: order.redCoins })}
        extra={
          <Space>
            <Button type="primary" onClick={() => navigate(WEB_URL.wallet)}>
              {t("page.shop.goWallet")}
            </Button>
            <Button onClick={() => navigate(WEB_URL.shop)}>
              {t("page.shop.buyMore")}
            </Button>
          </Space>
        }
      >
        {paidAt !== null && (
          <Text type="secondary">
            {t("page.shop.paidAt", {
              time: `${paidAt.date} · ${paidAt.time}`,
            })}
          </Text>
        )}
      </Result>
    );
  }

  if (order.status !== "pending") {
    return (
      <Result
        status="error"
        title={t("page.shop.failedTitle")}
        subTitle={t(FAILED_STATUS_KEY[order.status])}
        extra={
          <Button
            type="primary"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate(WEB_URL.shop)}
          >
            {t("page.shop.backToShop")}
          </Button>
        }
      />
    );
  }

  const qrSrc =
    payment === null || qrBroken
      ? null
      : payment.qrCode.startsWith("data:")
        ? payment.qrCode
        : `data:image/png;base64,${payment.qrCode}`;

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
      }}
    >
      <Title level={4} style={{ marginTop: 0 }}>
        <ClockCircleOutlined
          style={{ color: token.colorPrimary, marginRight: 8 }}
        />
        {t("page.shop.waitingTitle")}
      </Title>
      <Text type="secondary">{t("page.shop.waitingSubtitle")}</Text>

      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          gap: token.marginLG,
          marginTop: token.marginMD,
        }}
      >
        <div style={{ flex: 1, minWidth: 220 }}>
          <Space direction="vertical" size={4}>
            <Space size={8}>
              <Text type="secondary">{t("page.shop.amountLabel")}:</Text>
              <Text strong>{formatVnd(order.amountVnd)}</Text>
            </Space>
            <Space size={8}>
              <Text type="secondary">{t("page.shop.redCoinsLabel")}:</Text>
              <Space size={4}>
                <CoinIcon variant="red" size={16} />
                <Text strong>{order.redCoins}</Text>
              </Space>
            </Space>
            <Space size={8}>
              <Text type="secondary">{t("page.shop.orderCodeLabel")}:</Text>
              <Text>{order.orderCode}</Text>
            </Space>
          </Space>

          <Space
            direction="vertical"
            size={8}
            style={{ width: "100%", marginTop: token.marginMD }}
          >
            {payment !== null && (
              <Button
                type="primary"
                block
                icon={<LinkOutlined />}
                onClick={() =>
                  window.open(
                    payment.checkoutUrl,
                    "_blank",
                    "noopener,noreferrer",
                  )
                }
              >
                {t("page.shop.openCheckout")}
              </Button>
            )}
            <Button
              block
              icon={<ReloadOutlined />}
              loading={refreshing}
              onClick={onRefresh}
            >
              {t("page.shop.checkAgain")}
            </Button>
            <Button
              type="text"
              block
              icon={<ArrowLeftOutlined />}
              onClick={() => navigate(WEB_URL.shop)}
            >
              {t("page.shop.backToShop")}
            </Button>
          </Space>
        </div>

        {qrSrc !== null && (
          <div style={{ textAlign: "center" }}>
            <Text type="secondary" style={{ fontSize: 12, display: "block" }}>
              <QrcodeOutlined style={{ marginRight: 4 }} />
              {t("page.shop.qrTitle")}
            </Text>
            <img
              src={qrSrc}
              alt={t("page.shop.qrAlt")}
              onError={() => setQrBroken(true)}
              style={{
                width: 200,
                height: 200,
                marginTop: token.marginSM,
                borderRadius: token.borderRadiusSM,
                background: token.colorBgContainer,
                padding: 8,
              }}
            />
          </div>
        )}
      </div>

      <Alert
        type="info"
        showIcon
        style={{ marginTop: token.marginMD }}
        message={t("page.shop.pollHint")}
      />
    </Card>
  );
}
