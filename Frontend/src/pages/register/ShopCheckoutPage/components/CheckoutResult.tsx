import { Button, Flex, Grid, Result, Space, Tag, Typography } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import i18n from "@/i18n";
import { WEB_URL } from "@/routes";
import type { GetOrderStatusResult, PaymentOrderStatus } from "@/types";
import { convertISOToDate } from "@/utils";

const { Text } = Typography;
const { useBreakpoint } = Grid;

const FAILED_STATUS_KEY: Record<
  Exclude<PaymentOrderStatus, "pending" | "paid">,
  string
> = {
  cancelled: "page.shop.status.cancelled",
  expired: "page.shop.status.expired",
  failed: "page.shop.status.failed",
};

export interface CheckoutResultProps {
  order: GetOrderStatusResult;
}

/** Terminal checkout state: the order was paid or it failed/expired/cancelled. */
export default function CheckoutResult({ order }: CheckoutResultProps) {
  const { t } = useTranslation();
  const screens = useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();

  if (order.status === "paid") {
    const paidAt =
      order.paidAt === null
        ? null
        : convertISOToDate(order.paidAt, i18n.language);

    const actions = isMobile ? (
      <Flex vertical gap={8}>
        <Button type="primary" size="large" block onClick={() => navigate(WEB_URL.wallet)}>
          {t("page.shop.goWallet")}
        </Button>
        <Button size="large" block onClick={() => navigate(WEB_URL.shop)}>
          {t("page.shop.buyMore")}
        </Button>
      </Flex>
    ) : (
      <Space>
        <Button type="primary" onClick={() => navigate(WEB_URL.wallet)}>
          {t("page.shop.goWallet")}
        </Button>
        <Button onClick={() => navigate(WEB_URL.shop)}>
          {t("page.shop.buyMore")}
        </Button>
      </Space>
    );

    return (
      <Result
        status="success"
        title={t("page.shop.paidTitle")}
        subTitle={t(
          order.whiteCoins > 0
            ? "page.shop.paidSubtitleBonus"
            : "page.shop.paidSubtitle",
          { redCoins: order.redCoins, whiteCoins: order.whiteCoins },
        )}
        extra={actions}
      >
        <Flex vertical gap={8} align="center">
          <Space wrap>
            <Tag color="red" style={{ margin: 0 }}>
              +{order.redCoins} {t("page.shop.packageRedCoins")}
            </Tag>
            {order.whiteCoins > 0 && (
              <Tag color="gold" style={{ margin: 0 }}>
                +{order.whiteCoins} {t("page.shop.bonusCoinsLabel")}
              </Tag>
            )}
          </Space>
          {paidAt !== null && (
            <Text type="secondary">
              {t("page.shop.paidAt", {
                time: `${paidAt.date} · ${paidAt.time}`,
              })}
            </Text>
          )}
        </Flex>
      </Result>
    );
  }

  if (order.status === "pending") {
    return null;
  }

  return (
    <Result
      status="error"
      title={t("page.shop.failedTitle")}
      subTitle={t(FAILED_STATUS_KEY[order.status])}
      extra={
        <Button
          type="primary"
          size={isMobile ? "large" : "middle"}
          block={isMobile}
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate(WEB_URL.shop)}
        >
          {t("page.shop.backToShop")}
        </Button>
      }
    />
  );
}
