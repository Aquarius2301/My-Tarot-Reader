import {
  ErrorComponent,
  TarotCard,
  TarotDeck,
  type SpreadResultItem,
} from "@/components";
import { useCreateDrawForAuth, useGetLastDrawnCardForAuth } from "@/hooks/api";
import { TarotMeaningCard } from "@/pages/shared/tarot";
import { getErrorMessage } from "@/utils";
import { App, Button, Spin, Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Title } = Typography;

export default function AuthTarotPage() {
  const { t } = useTranslation();
  const { message } = App.useApp();
  const { data, isLoading, refetch } = useGetLastDrawnCardForAuth();
  const { mutate, isPending } = useCreateDrawForAuth();
  const [reDraw, setReDraw] = useState(false);

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    let selectedCard = selectedCards[0];
    setReDraw(false);
    mutate(
      {
        cardCode: selectedCard.cardCode,
        isReversed: selectedCard.isReversed,
      },
      {
        onError: (error) => {
          message.error(getErrorMessage(error));
        },
      },
    );
  };

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined)
    return <ErrorComponent type="server" onRetry={refetch} />;

  if (data == null || reDraw) {
    return (
      <div style={{ maxWidth: 960, margin: "0 auto" }}>
        <Title level={3} style={{ textAlign: "center" }}>
          {t("page.tarot.intro")}
        </Title>
        <TarotDeck limit={1} onConfirm={handleConfirm} />

        {isPending && <Spin fullscreen description={t("page.tarot.saving")} />}
      </div>
    );
  }

  const orientation = data.isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const name = t(`tarot.meaning.${data.cardCode}.name`);

  return (
    <div style={{ maxWidth: 720, margin: "0 auto", textAlign: "center" }}>
      <Typography.Title level={3}>{t("page.tarot.yourCard")}</Typography.Title>

      <div
        style={{
          display: "flex",
          justifyContent: "center",
          margin: "16px 0",
        }}
      >
        <TarotCard
          cardCode={data.cardCode}
          isUpright={!data.isReversed}
          isFlipped
          size="md"
        />
      </div>

      <Title level={4}>
        {name} · {orientation}
      </Title>

      <TarotMeaningCard cardCode={data.cardCode} isReversed={data.isReversed} />

      {/* <Card style={{ textAlign: "left", marginTop: 16 }}>
        {TAROT_SECTIONS.map((section) => {
          const key = meaningKey(section);
          const text = i18n.exists(key)
            ? t(key)
            : t(`tarot.placeholder.${section}`);
          return (
            <div key={section} style={{ marginBottom: 16 }}>
              <Typography.Text strong>
                {t(`tarot.section.${section}`)}
              </Typography.Text>
              <Typography.Paragraph style={{ marginTop: 4 }}>
                {text}
              </Typography.Paragraph>
            </div>
          );
        })}
      </Card> */}

      <Button onClick={() => setReDraw(true)} style={{ marginTop: 16 }}>
        {t("page.tarot.drawAgain")}
      </Button>
    </div>
  );
}
