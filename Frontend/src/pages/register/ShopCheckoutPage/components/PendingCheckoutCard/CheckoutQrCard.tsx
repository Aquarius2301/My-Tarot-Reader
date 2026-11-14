import {
  Alert,
  Button,
  Card,
  Flex,
  Grid,
  QRCode,
  Typography,
  theme,
} from "antd";
import { LinkOutlined, SyncOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import type { CheckoutPayload } from "@/utils";

const { Text, Title } = Typography;
const { useBreakpoint } = Grid;

export interface CheckoutQrCardProps {
  payment: CheckoutPayload | null;
}

/** PayOS QR card shown while an order is pending: QR, hint, and checkout link. */
export function CheckoutQrCard({ payment }: CheckoutQrCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const qrValue = payment?.qrCode.trim() ?? "";
  const checkoutUrl = payment?.checkoutUrl.trim() ?? "";
  const qrSize = isMobile ? 200 : 240;

  return (
    <Card style={{ height: "100%" }} styles={{ body: { height: "100%" } }}>
      <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
        <Flex justify="space-between" align="center" wrap gap={token.marginXS}>
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
          <Alert type="warning" showIcon title={t("page.shop.qrUnavailable")} />
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
  );
}
