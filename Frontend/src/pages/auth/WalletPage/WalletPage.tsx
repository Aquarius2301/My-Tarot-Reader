import { Spin, Typography } from "antd";
import { useTranslation } from "react-i18next";
import { ErrorComponent } from "@/components";
import { useGetWallet } from "@/hooks/api";
import {
  BalanceOverview,
  ConvertCard,
  WhiteCoinBatchesCard,
} from "./components";

const { Title, Text } = Typography;

/**
 * Wallet page: current white/red balances, the active white coin batches
 * ordered by expiry, and a red-to-white conversion form.
 */
export default function WalletPage() {
  const { t } = useTranslation();
  const { data, isLoading, refetch } = useGetWallet();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center", marginBottom: 4 }}>
        {t("page.wallet.title")}
      </Title>
      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 24 }}
      >
        {t("page.wallet.subtitle")}
      </Text>

      <BalanceOverview whiteCoin={data.whiteCoin} redCoin={data.redCoin} />
      <WhiteCoinBatchesCard batches={data.whiteCoinBatches} />
      <ConvertCard redCoin={data.redCoin} />
    </div>
  );
}