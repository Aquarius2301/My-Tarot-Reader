import { ResponsiveModal, TarotCard } from "@/components";
import { Button, Flex } from "antd";
import { RetweetOutlined } from "@ant-design/icons";
import { type TarotCardCode } from "@/constants";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import { TarotMeaningCard } from "@/pages/shared/tarot";

interface MeaningModalProps {
  selectedCard: TarotCardCode | null;
  onSelectedCard: (card: TarotCardCode | null) => void;
}

export default function MeaningModal({
  selectedCard,
  onSelectedCard,
}: MeaningModalProps) {
  const { t } = useTranslation();

  const [isReversed, setIsReversed] = useState(false);

  const orientation = isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const name = t(`tarot.meaning.${selectedCard}.name`);

  return (
    <ResponsiveModal
      title={
        <>
          {t("page.library.meaning", { card: name, orientation })}{" "}
          <Button onClick={() => setIsReversed(!isReversed)} type="text">
            <RetweetOutlined />
          </Button>
        </>
      }
      size="lg"
      open={!!selectedCard}
      onClose={() => onSelectedCard(null)}
    >
      <Flex vertical align="center">
        <div
          style={{
            display: "flex",
            justifyContent: "center",
            margin: "16px 0",
          }}
        >
          <TarotCard
            cardCode={selectedCard as TarotCardCode}
            isUpright={!isReversed}
            isFlipped
            size="lg"
          />
        </div>

        <TarotMeaningCard
          cardCode={selectedCard as TarotCardCode}
          isReversed={isReversed}
        />
      </Flex>
    </ResponsiveModal>
  );
}
