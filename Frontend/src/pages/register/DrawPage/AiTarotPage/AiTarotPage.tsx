import {
  useCreateAiTarotReading,
  useGetAiTarotSettings,
  useGetCurrentUser,
} from "@/hooks/api";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppTitle, ErrorView, type SpreadResultItem } from "@/components";
import { DrawCardsStep, SelectReadingStep } from "./components";
import { App, Spin } from "antd";
import { useLanguageStore } from "@/hooks/stores";
import { useNavigate } from "react-router-dom";
import { WEB_URL } from "@/routes";
import { getErrorMessage } from "@/utils";

interface AiTarotConfig {
  cardCount: number;
  questionType: string;
}

/** Two-step AI tarot reading: configure the spread, then draw the cards. */
export default function AiTarotPage() {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const navigate = useNavigate();
  const locale = useLanguageStore((s) => s.mode);

  const [config, setConfig] = useState<AiTarotConfig | null>(null);

  const { mutate, isPending } = useCreateAiTarotReading();
  const { data: user } = useGetCurrentUser();

  const {
    data: settings,
    isLoading,
    refetch,
    isError,
  } = useGetAiTarotSettings();

  const handleConfirm = (
    selectedCards: SpreadResultItem[],
    cardCount: number,
    questionType: string,
  ) => {
    mutate(
      {
        cardCount: cardCount,
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

  if (isError) return <ErrorView type="server" onRetry={refetch} />;

  return (
    <div>
      {!config ? (
        <>
          <AppTitle
            title={t("page.aiTarot.title")}
            subtitle={t("page.aiTarot.subTitle")}
            isLoading={isLoading}
          />
          <SelectReadingStep
            settings={settings}
            isLoading={isLoading}
            balance={user?.whiteCoin ?? 0}
            onNext={(cardCount, questionType) =>
              setConfig({ cardCount, questionType })
            }
          />
        </>
      ) : (
        <DrawCardsStep
          cardCount={config.cardCount}
          onConfirm={(selectedCards) =>
            handleConfirm(selectedCards, config.cardCount, config.questionType)
          }
          onBack={() => setConfig(null)}
        />
      )}

      {isPending && <Spin fullscreen description={t("page.aiTarot.saving")} />}
    </div>
  );
}
