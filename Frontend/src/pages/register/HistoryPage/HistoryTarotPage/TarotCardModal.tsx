import { ResponsiveModal, TarotCard, TarotMeaningCard } from "@/components";
import { Flex } from "antd";
import { type TarotCardCode } from "@/constants";
import { useTranslation } from "react-i18next";
import type { CardData } from "@/types";

interface TarotCardModalProps {
  open: boolean;
  onClose: () => void;
  selectedCard: CardData | null;
}

export function TarotCardModal({
  open,
  onClose,
  selectedCard,
}: TarotCardModalProps) {
  const { t } = useTranslation();

  if (!selectedCard) {
    return <ResponsiveModal open={open} onClose={onClose} />;
  }

  const { cardCode, isReversed } = selectedCard;
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
      <Flex vertical align="center" gap={16}>
        <TarotCard
          cardCode={cardCode as TarotCardCode}
          isUpright={!isReversed}
          isFlipped
          size="lg"
        />

        <TarotMeaningCard cardCode={cardCode} isReversed={isReversed} />
      </Flex>
    </ResponsiveModal>
  );
}
