import { Card, Divider, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { Coin } from "@/components";
import { BalanceOverviewLoading } from "./BalanceOverviewLoading";

const { Text } = Typography;

export interface BalanceOverviewProps {
  whiteCoin: number;
  redCoin: number;
  isLoading?: boolean;
}

/**
 * Shows the total white and red coin balances as two centered coin stats.
 * Mirrors the header CoinBadge styling but larger for the wallet page.
 */
export function BalanceOverview({
  whiteCoin,
  redCoin,
  isLoading = false,
}: BalanceOverviewProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  if (isLoading) {
    return <BalanceOverviewLoading />;
  }

  // const formatCoins = (value: number) => value.toLocaleString("vi-VN");

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
        <Text>{t("component.coin.white")}</Text>
        <Text style={{ fontSize: 28, fontWeight: 700, lineHeight: 1 }}>
          <Coin type="white" number={whiteCoin} isTooltip={false} />
        </Text>
      </div>
      <Divider vertical style={{ height: 84, margin: 0 }} />
      <div style={balanceStyle}>
        <Text>{t("component.coin.red")}</Text>
        <Text style={{ fontSize: 28, fontWeight: 700, lineHeight: 1 }}>
          <Coin type="red" number={redCoin} isTooltip={false} />
        </Text>
      </div>
    </Card>
  );
}
