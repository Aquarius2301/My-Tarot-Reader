import { TarotCard, TarotMeaningCard } from "@/components";
import type { GetLastDrawnCardForAuthResult } from "@/types";
import { Button, Flex } from "antd";
import Title from "antd/es/typography/Title";
import { useTranslation } from "react-i18next";
import { TarotResultLoading } from "./TarotResultLoading";

export interface TarotResultProps {
  data?: GetLastDrawnCardForAuthResult | null;
  isLoading?: boolean;
  onReDraw: () => void;
}

export function TarotResult({ data, isLoading, onReDraw }: TarotResultProps) {
  const { t } = useTranslation();

  if (isLoading) {
    return <TarotResultLoading />;
  }

  if (!data) {
    return null;
  }

  const orientation = data.isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const name = t(`tarot.meaning.${data.cardCode}.name`);
  return (
    <Flex vertical align="center" style={{ textAlign: "center" }}>
      <Title level={3}>{t("page.tarot.yourCard")}</Title>

      <TarotCard
        cardCode={data.cardCode}
        isUpright={!data.isReversed}
        isFlipped
        size="md"
      />

      <Title level={4}>
        {name} · {orientation}
      </Title>

      <TarotMeaningCard cardCode={data.cardCode} isReversed={data.isReversed} />

      <Button onClick={() => onReDraw()} style={{ marginTop: 16 }}>
        {t("page.tarot.drawAgain")}
      </Button>
    </Flex>
  );
}
