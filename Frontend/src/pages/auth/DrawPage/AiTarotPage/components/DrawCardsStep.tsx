import { TarotDeck, type SpreadResultItem } from "@/components";
import { useCreateAiTarotReading } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/stores";
import { WEB_URL } from "@/routes";
import { getErrorMessage } from "@/utils";
import { App, Button, Flex, Spin, Typography } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import type { AiTarotCardCount, AiTarotQuestionType } from "@/constants";

const { Text } = Typography;

export interface DrawCardsStepProps {
  cardCount: AiTarotCardCount;
  questionType: AiTarotQuestionType;
  onBack: () => void;
}

/** Step 2: draw the spread on the TarotDeck and create the AI reading. */
export default function DrawCardsStep({
  cardCount,
  questionType,
  onBack,
}: DrawCardsStepProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const navigate = useNavigate();
  const locale = useLanguageStore((s) => s.mode);
  const { mutate, isPending } = useCreateAiTarotReading();

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    mutate(
      {
        cardCount,
        type: questionType,
        locale,
        cards: selectedCards.map((card) => ({
          cardCode: card.cardCode,
          isReversed: card.isReversed,
        })),
      },
      {
        onSuccess: (result) => {
          navigate(`${WEB_URL.aiTarotResult}/${result.id}`);
        },
        onError: (error) => {
          message.error(getErrorMessage(error));
        },
      },
    );
  };

  return (
    <div>
      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 16 }}
      >
        {t("page.aiTarot.step2.subtitle", { count: cardCount })}
      </Text>

      <TarotDeck limit={cardCount} onConfirm={handleConfirm} />

      <Flex justify="center" style={{ marginTop: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
          {t("page.aiTarot.back")}
        </Button>
      </Flex>

      {isPending && <Spin fullscreen description={t("page.aiTarot.saving")} />}
    </div>
  );
}