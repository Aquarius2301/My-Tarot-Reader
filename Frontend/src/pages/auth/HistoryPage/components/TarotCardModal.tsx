import { ResponsiveModal, TarotCard } from "@/components";
import { Flex } from "antd";
import { type TarotCardCode } from "@/constants";
import { useTranslation } from "react-i18next";
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
