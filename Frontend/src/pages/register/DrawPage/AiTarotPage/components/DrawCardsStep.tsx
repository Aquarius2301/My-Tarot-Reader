import { AppTitle, TarotDeck, type SpreadResultItem } from "@/components";
import { Button, Flex } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";

interface DrawCardsStepProps {
  cardCount: number;
  onBack: () => void;
  onConfirm: (selectedCards: SpreadResultItem[]) => void;
}

/** Step 2: draw the spread on the TarotDeck and create the AI reading. */
export function DrawCardsStep({
  cardCount,
  onBack,
  onConfirm,
}: DrawCardsStepProps) {
  const { t } = useTranslation();

  return (
    <div>
      {/* <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 16 }}
      >
        {t("page.aiTarot.step2.subtitle", { count: cardCount })}
      </Text> */}
      <AppTitle
        title={t("page.aiTarot.step2.subtitle", {
          count: cardCount,
        })}
      />

      <TarotDeck limit={cardCount} onConfirm={onConfirm} />

      <Flex justify="center" style={{ marginTop: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
          {t("page.aiTarot.back")}
        </Button>
      </Flex>
    </div>
  );
}
