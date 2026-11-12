import { TarotCard, TarotMeaningCard } from "@/components";
import type { GetLastDrawnCardForAuthResult } from "@/types";
import { Button, Flex } from "antd";
import Title from "antd/es/typography/Title";
import { useTranslation } from "react-i18next";

export interface TarotResultProps {
  data: GetLastDrawnCardForAuthResult;
  onReDraw: () => void;
}

export function TarotResult({ data, onReDraw }: TarotResultProps) {
  const { t } = useTranslation();

  const orientation = data.isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const name = t(`tarot.meaning.${data.cardCode}.name`);
  return (
    <Flex vertical align="center" style={{ textAlign: "center" }}>
      <Title level={3}>{t("page.tarot.yourCard")}</Title>

      <div
        style={{
          display: "flex",
          justifyContent: "center",
          margin: "16px 0",
        }}
      >
        <TarotCard
          cardCode={data.cardCode}
          isUpright={!data.isReversed}
          isFlipped
          size="md"
        />
      </div>

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
