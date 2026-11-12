import {
  Alert,
  Button,
  Card,
  Col,
  Flex,
  Grid,
  QRCode,
  Row,
  Typography,
  theme,
} from "antd";
import {
  ArrowLeftOutlined,
  LinkOutlined,
  SyncOutlined,
} from "@ant-design/icons";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { Coin, CopyButton } from "@/components";
import { WEB_URL } from "@/routes";
import type { GetOrderStatusResult } from "@/types";
import { formatVnd, type CheckoutPayload } from "@/utils";

const { Text, Title } = Typography;
const { useBreakpoint } = Grid;

export interface PendingCheckoutCardProps {
  order: GetOrderStatusResult;
  payment: CheckoutPayload | null;
}

/**
 * Pending checkout: the PayOS QR and payment action come first (also on
 * mobile), followed by the order summary. The tab keeps polling the order
 * status until it settles.
 */
export default function PendingCheckoutCard({
  order,
  payment,
}: PendingCheckoutCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const screens = useBreakpoint();
  const isMobile = !screens.md;
  const navigate = useNavigate();

  const qrValue = payment?.qrCode.trim() ?? "";
  const checkoutUrl = payment?.checkoutUrl.trim() ?? "";
  const qrSize = isMobile ? 200 : 240;

  const infoRow = (label: string, value: ReactNode) => (
    <Flex justify="space-between" align="center" gap={token.marginSM}>
      <Text type="secondary">{label}</Text>
      <Flex align="center" gap={token.marginXS}>
        {value}
      </Flex>
    </Flex>
  );

  return (
    <Row gutter={[token.marginMD, token.marginMD]}>
      <Col xs={24} md={11}>
        <Card style={{ height: "100%" }} styles={{ body: { height: "100%" } }}>
          <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
            <Flex
              justify="space-between"
              align="center"
              wrap
              gap={token.marginXS}
            >
              <Title level={4} style={{ margin: 0 }}>
                <SyncOutlined
                  spin
                  style={{ color: token.colorPrimary, marginRight: 8 }}
                />
                {t("page.shop.waitingTitle")}
              </Title>
              <Text type="secondary" style={{ fontSize: 12 }}>
                {t("page.shop.autoRefreshing")}
              </Text>
            </Flex>

            {qrValue !== "" && (
              <div style={{ textAlign: "center" }}>
                <Text
                  type="secondary"
                  style={{ display: "block", marginBottom: token.marginSM }}
                >
                  {t("page.shop.qrTitle")}
                </Text>
                <div style={{ display: "flex", justifyContent: "center" }}>
                  <QRCode value={qrValue} size={qrSize} icon="/logo.png" />
                </div>
              </div>
            )}

            {qrValue === "" && checkoutUrl === "" ? (
              <Alert
                type="warning"
                showIcon
                title={t("page.shop.qrUnavailable")}
              />
            ) : (
              <Text type="secondary" style={{ textAlign: "center" }}>
                {t("page.shop.waitingSubtitle")}
              </Text>
            )}

            <div style={{ flex: 1 }} />

            {checkoutUrl !== "" && (
              <Button
                type="primary"
                size="large"
                block
                icon={<LinkOutlined />}
                onClick={() =>
                  window.open(checkoutUrl, "_blank", "noopener,noreferrer")
                }
              >
                {t("page.shop.openCheckout")}
              </Button>
            )}
          </Flex>
        </Card>
      </Col>
      <Col xs={24} md={13}>
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
                  <Coin
                    type="white"
                    number={order.whiteCoins}
                    isTooltip={false}
                  />,
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
      </Col>
    </Row>
  );
}
