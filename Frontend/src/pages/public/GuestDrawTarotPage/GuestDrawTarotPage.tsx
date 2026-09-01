import { TarotCard, TarotDeck, type SpreadResultItem } from "@/components";
import { TAROT_SECTIONS, WEB_URL } from "@/constants";
import {
  useCreateDrawForGuest,
  useGetAvailableTimeForGuest,
} from "@/hooks/api";
import { convertSecondsToHours, getErrorMessage } from "@/utils";
import { App, Button, Card, Spin, Typography } from "antd";
import { Trans, useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

const { Title } = Typography;

export default function GuestDrawTarotPage() {
  const { t, i18n } = useTranslation();
  const { data, isLoading } = useGetAvailableTimeForGuest();
  const { mutate, isPending } = useCreateDrawForGuest();
  const { message } = App.useApp();
  const navigate = useNavigate();

  const handleConfirm = (selectedCards: SpreadResultItem[]) => {
    let selectedCard = selectedCards[0];
    mutate(
      {
        cardCode: selectedCard.cardCode,
        isReversed: selectedCard.isReversed,
      },
      {
        onError: (err) => {
          message.error(getErrorMessage(err));
        },
      },
    );
  };

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (!data) return null;

  if (data.remainingSeconds <= 0) {
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

  const { hours, minutes } = convertSecondsToHours(data.remainingSeconds);
  return (
    <div style={{ maxWidth: 720, margin: "0 auto", textAlign: "center" }}>
      <Trans
        i18nKey="tarot.draw.cooldown"
        values={{ hours, minutes }}
        components={{
          btn: (
            <Button
              style={{ padding: 2 }}
              type="link"
              onClick={() => {
                navigate(WEB_URL.LOGIN);
              }}
            />
          ),
        }}
      />

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
    </div>
  );
}
