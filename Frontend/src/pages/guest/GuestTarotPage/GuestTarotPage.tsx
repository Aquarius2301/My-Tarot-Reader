import {
  AppTitle,
  ErrorView,
  TarotDeck,
  type SpreadResultItem,
} from "@/components";
import { App } from "antd";
import { useTranslation } from "react-i18next";
import { GuestTarotResult } from "./GuestTarotResult";
import {
  useCreateDrawForGuest,
  useGetLastDrawnCardForGuest,
} from "@/hooks/api";
import { GuestTarotPageLoading } from "./GuestTarotPageLoading";
import { getErrorMessage } from "@/utils";

export default function GuestTarotPage() {
  const { t } = useTranslation();

  const { message } = App.useApp();

  const { mutate, isPending } = useCreateDrawForGuest();

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    let selectedCard = selectedCards[0];
    mutate(
      {
        cardCode: selectedCard.cardCode,
        isReversed: selectedCard.isReversed,
      },
      {
        onError: (error) => message.error(getErrorMessage(error)),
      },
    );
  };

  const { data, isLoading, refetch } = useGetLastDrawnCardForGuest();

  if (isLoading || isPending) {
    return <GuestTarotPageLoading />;
  }

  if (data === undefined) return <ErrorView type="server" onRetry={refetch} />;

  return data ? (
    <GuestTarotResult data={data} />
  ) : (
    <>
      <AppTitle
        title={t("page.tarot.title")}
        subtitle={t("page.tarot.subTitle")}
      />
      <TarotDeck limit={1} onConfirm={handleConfirm} />
    </>
  );
}
