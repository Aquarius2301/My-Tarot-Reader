import {
  AppTitle,
  ErrorView,
  TarotDeck,
  type SpreadResultItem,
} from "@/components";
import { useCreateDrawForAuth, useGetLastDrawnCardForAuth } from "@/hooks/api";
import { getErrorMessage } from "@/utils";
import { App } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { TarotResult } from "./components";

export default function TarotPage() {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const { data, isLoading, refetch, isError } = useGetLastDrawnCardForAuth();
  const { mutate, isPending } = useCreateDrawForAuth();
  const [reDraw, setReDraw] = useState(false);

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    let selectedCard = selectedCards[0];
    setReDraw(false);
    mutate(
      {
        cardCode: selectedCard.cardCode,
        isReversed: selectedCard.isReversed,
      },
      {
        onError: (error) => {
          message.error(getErrorMessage(error));
        },
      },
    );
  };

  if (isError) return <ErrorView type="server" onRetry={refetch} />;

  const isPageLoading = isLoading || isPending;

  const isDraw = !(isPageLoading || data) || reDraw;

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
        <TarotResult
          data={data}
          isLoading={isPageLoading}
          onReDraw={() => setReDraw(true)}
        />
      )}
    </>
  );
}
