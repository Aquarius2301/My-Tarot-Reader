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
import { TarotPageLoading } from "./TarotPageLoading";
import { TarotResult } from "./TarotResult";

export default function TarotPage() {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const { data, isLoading, refetch } = useGetLastDrawnCardForAuth();
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

  if (isLoading || isPending) {
    return <TarotPageLoading />;
  }

  if (data === undefined) return <ErrorView type="server" onRetry={refetch} />;

  return data == null || reDraw ? (
    <>
      <AppTitle
        title={t("page.tarot.title")}
        subtitle={t("page.tarot.subTitle")}
      />
      <TarotDeck limit={1} onConfirm={handleConfirm} />
    </>
  ) : (
    <TarotResult data={data} onReDraw={() => setReDraw(true)} />
  );
}
