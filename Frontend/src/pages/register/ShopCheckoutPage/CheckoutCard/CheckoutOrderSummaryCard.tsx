import { Alert, Button, Card, Flex, Typography, theme } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { Coin, CopyButton } from "@/components";
import { WEB_URL } from "@/routes";
import type { GetOrderStatusResult } from "@/types";
import { formatVnd } from "@/utils";

const { Text, Title } = Typography;

export interface CheckoutOrderSummaryCardProps {
  order: GetOrderStatusResult;
}

/** Order summary card: amount, granted coins, order code, and polling hint. */
export function CheckoutOrderSummaryCard({
  order,
}: CheckoutOrderSummaryCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const navigate = useNavigate();

  const infoRow = (label: string, value: ReactNode) => (
    <Flex justify="space-between" align="center" gap={token.marginSM}>
      <Text type="secondary">{label}</Text>
      <Flex align="center" gap={token.marginXS}>
        {value}
      </Flex>
    </Flex>
  );

  return (
    <Card style={{ height: "100%" }} styles={{ body: { height: "100%" } }}>
      <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
        <Title level={4} style={{ margin: 0 }}>
          {t("page.shop.orderSummary")}
        </Title>

        <div>
          <Text type="secondary">{t("page.shop.amountLabel")}</Text>
          <div>
            <Text strong style={{ fontSize: 32, lineHeight: 1.2 }}>
              {formatVnd(order.amountVnd)}
            </Text>
          </div>
        </div>

        <Flex vertical gap={token.marginSM}>
          {infoRow(
            t("page.shop.redCoinsLabel"),
            <Coin type="red" number={order.redCoins} isTooltip={false} />,
          )}

          {order.whiteCoins > 0 &&
            infoRow(
              t("page.shop.bonusCoinsLabel"),
              <Coin type="white" number={order.whiteCoins} isTooltip={false} />,
            )}

          {infoRow(
            t("page.shop.orderCodeLabel"),
            <>
              <Text>{order.orderCode}</Text>
              <CopyButton
                size="small"
                text={String(order.orderCode)}
                style={{ paddingInline: 8, height: 24 }}
              />
            </>,
          )}
        </Flex>

        <div style={{ flex: 1 }} />

        <Flex vertical gap={token.marginSM}>
          <Button
            type="text"
            block
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate(WEB_URL.shop)}
          >
            {t("page.shop.backToShop")}
          </Button>
          <Alert type="info" showIcon title={t("page.shop.pollHint")} />
        </Flex>
      </Flex>
    </Card>
  );
}
