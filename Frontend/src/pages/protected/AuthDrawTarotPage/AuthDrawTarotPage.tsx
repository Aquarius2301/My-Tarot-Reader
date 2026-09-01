import { TarotCard, TarotDeck, type SpreadResultItem } from "@/components";
import { TAROT_SECTIONS } from "@/constants";
import { useCreateDrawForAuth, useGetLastDrawnCardForAuth } from "@/hooks/api";
import { getErrorMessage } from "@/utils";
import { App, Button, Card, Spin, Typography } from "antd";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const { Title } = Typography;

export default function AuthDrawTarotPage() {
  const { t, i18n } = useTranslation();
  const { data, isLoading } = useGetLastDrawnCardForAuth();
  const { mutate, isPending } = useCreateDrawForAuth();
  const { message } = App.useApp();

  const [reDraw, setReDraw] = useState(false);

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    let selectedCard = selectedCards[0];
    mutate(
      {
        cardCode: selectedCard.cardCode,
        isReversed: selectedCard.isReversed,
      },
      {
        onSuccess: () => {
          setReDraw(false);
        },
        onError: (err) => {
          message.error(getErrorMessage(err));
        },
      },
    );
  };

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (!data || reDraw) {
    return (
      <div style={{ maxWidth: 960, margin: "0 auto" }}>
        <Title level={3} style={{ textAlign: "center" }}>
          {t("tarot.draw.intro")}
        </Title>
        <TarotDeck limit={1} onConfirm={handleConfirm} />

        {isPending && <Spin fullscreen description={t("tarot.draw.saving")} />}
      </div>
    );
  }

  const orientation = data.isReversed
    ? t("tarot.draw.reversed")
    : t("tarot.draw.upright");

  const name = t(`tarot.meaning.${data.cardCode}.name`);

  const meaningKey = (section: string) =>
    `tarot.meaning.${data.cardCode}.${data.isReversed ? "reversed" : "upright"}.${section}`;

  return (
    <div style={{ maxWidth: 720, margin: "0 auto", textAlign: "center" }}>
      <Typography.Title level={3}>{t("tarot.draw.yourCard")}</Typography.Title>

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

      <Card style={{ textAlign: "left", marginTop: 16 }}>
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
      </Card>

      <Button
        type="primary"
        style={{ marginTop: 24 }}
        onClick={() => setReDraw(true)}
      >
        {t("tarot.draw.drawAgain")}
      </Button>
    </div>
  );
}
