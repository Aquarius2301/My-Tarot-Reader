import {
  AppTitle,
  ErrorView,
  TarotDeck,
  type SpreadResultItem,
} from "@/components";
import { App } from "antd";
import { useTranslation } from "react-i18next";
import {
  useCreateDrawForGuest,
  useGetLastDrawnCardForGuest,
} from "@/hooks/api";
import { getErrorMessage } from "@/utils";
import { GuestTarotResult } from "./components";

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

  const { data, isLoading, refetch, isError } = useGetLastDrawnCardForGuest();

  if (isError) return <ErrorView type="server" onRetry={refetch} />;

  const isPageLoading = isLoading || isPending;
  const isDraw = !(isPageLoading || data); // = !isPageLoading && !data

  return (
    <>
      {isDraw ? (
        <>
          <AppTitle
            title={t("page.tarot.title")}
            subtitle={t("page.tarot.subTitle")}
          />
          <TarotDeck limit={1} onConfirm={handleConfirm} />
        </>
      ) : (
        <GuestTarotResult data={data} isLoading={isPageLoading} />
      )}
    </>
  );
}
