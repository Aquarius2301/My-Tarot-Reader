import { TarotDeck, type SpreadResultItem } from "@/components";
import { getCrossroadsCardCount, type CrossroadsTimeFrame } from "@/constants";
import { useCreateCrossroadsReading } from "@/hooks/api";
import { useLanguageStore } from "@/hooks/stores";
import { WEB_URL } from "@/routes";
import { getErrorMessage } from "@/utils";
import { App, Button, Card, Flex, Spin, Tag, Typography } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

const { Text, Paragraph } = Typography;

/** The decision the user described in step 1, replayed above the deck. */
export interface CrossroadsReadingInput {
  question: string;
  options: string[];
  timeFrame?: CrossroadsTimeFrame;
}

export interface CrossroadsDrawCardsStepProps {
  input: CrossroadsReadingInput;
  onBack: () => void;
}

/**
 * Step 2: draw the crossroads spread (3 cards per option plus one closing card)
 * and create the reading with the inputs collected in step 1.
 */
export default function CrossroadsDrawCardsStep({
  input,
  onBack,
}: CrossroadsDrawCardsStepProps) {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const navigate = useNavigate();
  const locale = useLanguageStore((s) => s.mode);
  const { mutate, isPending } = useCreateCrossroadsReading();

  const options = input.options.filter(Boolean);
  const cardCount = getCrossroadsCardCount(options.length);

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    mutate(
      {
        locale,
        question: input.question,
        options,
        timeFrame: input.timeFrame,
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
      <Card style={{ marginBottom: 16 }}>
        <Paragraph style={{ marginBottom: 8 }}>
          <Text strong>{t("page.aiDeepTarot.result.yourQuestion")}</Text>{" "}
          {input.question}
        </Paragraph>
        <Paragraph type="secondary" style={{ margin: 0 }}>
          <Text strong>{t("page.aiDeepTarot.result.yourOptions")}</Text>
        </Paragraph>
        <Flex gap={4} wrap style={{ marginTop: 8 }}>
          {options.map((option, index) => (
            <Tag key={`${option}-${index}`} color="purple">
              {option}
            </Tag>
          ))}
        </Flex>
      </Card>

      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 16 }}
      >
        {t("page.aiDeepTarot.spreads.crossroads.draw.subtitle")}
      </Text>

      <TarotDeck limit={cardCount} onConfirm={handleConfirm} />

      <Flex justify="center" style={{ marginTop: 16 }}>
        <Button icon={<ArrowLeftOutlined />} onClick={onBack}>
          {t("page.aiDeepTarot.common.back")}
        </Button>
      </Flex>

      {isPending && (
        <Spin
          fullscreen
          description={t("page.aiDeepTarot.common.saving")}
        />
      )}
    </div>
  );
}
