import { TarotCard, TarotMeaningCard } from "@/components";
import { WEB_URL } from "@/routes";
import type { GetLastDrawnCardForGuestResult } from "@/types";
import { convertSecondsToHours } from "@/utils";
import { Button, Flex } from "antd";
import Title from "antd/es/typography/Title";
import { Trans, useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

export interface GuestTarotResultProps {
  data: GetLastDrawnCardForGuestResult;
}

export function GuestTarotResult({ data }: GuestTarotResultProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const orientation = data.isReversed
    ? t("tarot.position.reversed")
    : t("tarot.position.upright");

  const name = t(`tarot.meaning.${data.cardCode}.name`);

  const { hours, minutes } = convertSecondsToHours(data.remainingSeconds);

  return (
    <Flex vertical align="center" style={{ textAlign: "center" }}>
      <Trans
        i18nKey="page.tarot.cooldown"
        values={{ hours, minutes }}
        components={{
          btn: (
            <Button
              style={{ padding: 2 }}
              type="link"
              onClick={() => {
                navigate(WEB_URL.login);
              }}
            />
          ),
        }}
      />
      <Title level={3}>{t("page.tarot.yourCard")}</Title>
      <Flex>
        <TarotCard
          cardCode={data.cardCode}
          isUpright={!data.isReversed}
          isFlipped
          size="md"
        />
      </Flex>
      <Title level={4}>
        {name} · {orientation}
      </Title>
      <TarotMeaningCard cardCode={data.cardCode} isReversed={data.isReversed} />
    </Flex>
  );
}
