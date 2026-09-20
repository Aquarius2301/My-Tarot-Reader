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

  // Keep the last shown card so the drawer content stays translated during the
  // exit animation (selectedCard is nulled on close before the animation ends).
  const [lastCard, setLastCard] = useState<TarotCardCode | null>(selectedCard);
  if (selectedCard !== null && selectedCard !== lastCard) {
    setLastCard(selectedCard);
  }
  const displayCard = selectedCard ?? lastCard;

  if (!displayCard) {
    return <ResponsiveModal open={!!selectedCard} onClose={() => onSelectedCard(null)} />;
  }

  const orientation = isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const name = t(`tarot.meaning.${displayCard}.name`);

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
            cardCode={displayCard}
            isUpright={!isReversed}
            isFlipped
            size="lg"
          />
        </div>

        <TarotMeaningCard cardCode={displayCard} isReversed={isReversed} />
      </Flex>
    </ResponsiveModal>
  );
}
