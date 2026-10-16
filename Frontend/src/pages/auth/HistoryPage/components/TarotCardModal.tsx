import { ResponsiveModal, TarotCard } from "@/components";
import { Flex } from "antd";
import { type TarotCardCode } from "@/constants";
import { useTranslation } from "react-i18next";
import { useState } from "react";
import type { CardData } from "@/types";
import { TarotMeaningCard } from "@/pages/shared/tarot";

interface TarotCardModalProps {
  open: boolean;
  onClose: () => void;
  selectedCard: CardData | null;
}

export default function TarotCardModal({
  open,
  onClose,
  selectedCard,
}: TarotCardModalProps) {
  const { t } = useTranslation();

  // Keep the last shown card so the drawer content stays translated during the
  // exit animation (selectedCard is nulled on close before the animation ends).
  const [lastCard, setLastCard] = useState<CardData | null>(selectedCard);
  if (selectedCard !== null && selectedCard !== lastCard) {
    setLastCard(selectedCard);
  }
  const displayCard = selectedCard ?? lastCard;

  if (!displayCard) {
    return <ResponsiveModal open={open} onClose={onClose} />;
  }

  const { cardCode, isReversed } = displayCard;
  const orientation = isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");
  const name = t(`tarot.meaning.${cardCode}.name`);

  return (
    <ResponsiveModal
      title={name + " · " + orientation}
      open={open}
      onClose={onClose}
      size="lg"
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
            cardCode={cardCode as TarotCardCode}
            isUpright={!isReversed}
            isFlipped
            size="lg"
          />
        </div>

        <TarotMeaningCard cardCode={cardCode} isReversed={isReversed} />
      </Flex>
    </ResponsiveModal>
  );
}
