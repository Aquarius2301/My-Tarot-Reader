import { Button, Flex, Grid, Result, Space, Tag, Typography } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import i18n from "@/i18n";
import { WEB_URL } from "@/routes";
import type { GetOrderStatusResult } from "@/types";
import { convertISOToDate } from "@/utils";

const { Text } = Typography;
const { useBreakpoint } = Grid;

export interface CheckoutPaidResultProps {
  order: GetOrderStatusResult;
}

/** Terminal success state: the order was paid and the coins were credited. */
export function CheckoutPaidResult({ order }: CheckoutPaidResultProps) {
  const { t } = useTranslation();
  const screens = useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();

  const paidAt =
    order.paidAt === null
      ? null
      : convertISOToDate(order.paidAt, i18n.language);

  const actions = isMobile ? (
    <Flex vertical gap={8}>
      <Button
        type="primary"
        size="large"
        block
        onClick={() => navigate(WEB_URL.wallet)}
      >
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
