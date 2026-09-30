import { TarotDeck, type SpreadResultItem } from "@/components";
import { useCreateAiDeepTarotReading } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/stores";
import { WEB_URL } from "@/routes";
import { getErrorMessage } from "@/utils";
import { App, Button, Flex, Spin, Typography } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { AI_DEEP_TAROT_CARD_COUNTS } from "@/constants";

const { Text } = Typography;

export interface DrawCardsStepProps {
  onBack: () => void;
}

/** Step 2: draw the 12 houses spread on the TarotDeck and create the reading. */
export default function DrawCardsStep({ onBack }: DrawCardsStepProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const navigate = useNavigate();
  const locale = useLanguageStore((s) => s.mode);
  const { mutate, isPending } = useCreateAiDeepTarotReading();

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    mutate(
      {
        topic: "twelveHouses",
        locale,
        cards: selectedCards.map((card) => ({
          cardCode: card.cardCode,
          isReversed: card.isReversed,
        })),
      },
      {
        onSuccess: (result) => {
          navigate(`${WEB_URL.aiDeepTarotResult}/${result.id}`);
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
        {t("page.aiDeepTarot.draw.subtitle")}
      </Text>

      <TarotDeck
        limit={AI_DEEP_TAROT_CARD_COUNTS.twelveHouses}
        onConfirm={handleConfirm}
      />

      <Flex justify="center" style={{ marginTop: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
          {t("page.aiDeepTarot.back")}
        </Button>
      </Flex>

      {isPending && <Spin fullscreen description={t("page.aiDeepTarot.saving")} />}
    </div>
  );
}
