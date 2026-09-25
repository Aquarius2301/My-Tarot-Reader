import { Card, Divider, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";

const { Text } = Typography;

export interface BalanceOverviewProps {
  whiteCoin: number;
  redCoin: number;
}

/**
 * Shows the total white and red coin balances as two centered coin stats.
 * Mirrors the header CoinBadge styling but larger for the wallet page.
 */
export default function BalanceOverview({
  whiteCoin,
  redCoin,
}: BalanceOverviewProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const formatCoins = (value: number) => value.toLocaleString("vi-VN");

  const balanceStyle = {
    flex: 1,
    display: "flex" as const,
    flexDirection: "column" as const,
    alignItems: "center" as const,
    gap: token.marginXS,
  };

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        boxShadow: token.boxShadowSecondary,
        marginBottom: 24,
      }}
      styles={{
        body: {
          display: "flex",
          alignItems: "center",
          padding: `${token.paddingLG}px ${token.marginLG}px`,
        },
      }}
    >
      <div style={balanceStyle}>
        <CoinIcon variant="white" size={30} />
        <Text type="secondary">{t("component.mainLayout.whiteCoin")}</Text>
        <Text style={{ fontSize: 28, fontWeight: 700, lineHeight: 1 }}>
          {formatCoins(whiteCoin)}
        </Text>
      </div>
      <Divider type="vertical" style={{ height: 84, margin: 0 }} />
      <div style={balanceStyle}>
        <CoinIcon variant="red" size={30} />
        <Text type="secondary">{t("component.mainLayout.redCoin")}</Text>
        <Text style={{ fontSize: 28, fontWeight: 700, lineHeight: 1 }}>
          {formatCoins(redCoin)}
        </Text>
      </div>
    </Card>
  );
}