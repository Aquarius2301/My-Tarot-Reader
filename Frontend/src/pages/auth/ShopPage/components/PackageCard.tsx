import { Button, Card, Space, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";
import type { GetPackagesItem } from "@/types";
import { formatVnd } from "@/utils";

const { Title, Text } = Typography;

export interface PackageCardProps {
  pkg: GetPackagesItem;
  loading: boolean;
  disabled: boolean;
  onBuy: () => void;
}

/**
 * One purchasable red-coin package: how many red coins it grants, its VND
 * price, and the button that starts a PayOS order for it.
 */
export default function PackageCard({
  pkg,
  loading,
  disabled,
  onBuy,
}: PackageCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
        height: "100%",
        textAlign: "center",
      }}
      styles={{ body: { padding: token.paddingLG } }}
    >
      <Space direction="vertical" size={8} style={{ width: "100%" }}>
        <Space size={8} align="center" style={{ justifyContent: "center" }}>
          <CoinIcon variant="red" size={24} />
          <Title level={3} style={{ margin: 0 }}>
            {pkg.redCoins}
          </Title>
        </Space>
        <Text type="secondary">{t("page.shop.packageRedCoins")}</Text>
        <Title level={4} style={{ margin: "8px 0 0" }}>
          {formatVnd(pkg.priceVnd)}
        </Title>
        <Button
          type="primary"
          block
          loading={loading}
          disabled={disabled}
          onClick={onBuy}
          style={{ marginTop: 8 }}
        >
          {t("page.shop.buyButton")}
        </Button>
      </Space>
    </Card>
  );
}
