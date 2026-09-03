import { CARD_COUNT_OPTIONS, type CardCount } from "@/constants";
import { Flex, Select } from "antd";
import { useTranslation } from "react-i18next";

export interface CardCountSelectProps {
  cardCount: CardCount;
  onCardCountChange: (CardCount: CardCount) => void;
}

export default function CardCountSelect({
  cardCount,
  onCardCountChange,
}: CardCountSelectProps) {
  const { t } = useTranslation();

  const options = CARD_COUNT_OPTIONS.map((value) => ({
    value,
    label: t(`select.cardCount.${value}`),
  }));

  return (
    <Flex vertical gap={4} style={{ flex: 1, minWidth: 180 }}>
      <label htmlFor="ai-draw-card-count" style={{ fontWeight: 600 }}>
        {t("select.cardCountLabel")}
      </label>
      <Select<CardCount>
        id="ai-draw-card-count"
        value={cardCount}
        style={{ width: "100%" }}
        options={options}
        onChange={onCardCountChange}
      />
    </Flex>
  );
}
