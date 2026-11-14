import { useTranslation } from "react-i18next";
import { AppTitle, ErrorView } from "@/components";
import { useGetWallet } from "@/hooks/api";
import {
  BalanceOverview,
  ConvertCard,
  WhiteCoinBatchesCard,
} from "./components";

/**
 * Wallet page: current white/red balances, the active white coin batches
 * ordered by expiry, and a red-to-white conversion form.
 */
export default function WalletPage() {
  const { t } = useTranslation();
  const { data, isLoading, refetch } = useGetWallet();

  if (!isLoading && data === undefined) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  return (
    <div>
      <AppTitle
        title={t("page.wallet.title")}
        subtitle={t("page.wallet.subtitle")}
        isLoading={isLoading}
      />

      <BalanceOverview
        whiteCoin={data?.whiteCoin}
        redCoin={data?.redCoin}
        isLoading={isLoading}
      />
      <WhiteCoinBatchesCard
        batches={data?.whiteCoinBatches}
        isLoading={isLoading}
      />
      <ConvertCard redCoin={data?.redCoin} isLoading={isLoading} />
    </div>
  );
}
