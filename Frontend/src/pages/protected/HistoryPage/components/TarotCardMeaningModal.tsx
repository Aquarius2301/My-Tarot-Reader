import { ResponsiveModal, TarotCard } from "@/components";
import { TAROT_SECTIONS, type TarotCardCode } from "@/constants";
import { Card, Tag, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";

export interface TarotCardMeaningModalProps {
  open: boolean;
  onClose: () => void;
  cardCode?: TarotCardCode;
  isReversed: boolean;
}

export default function TarotCardMeaningModal({
  open,
  onClose,
  cardCode,
  isReversed,
}: TarotCardMeaningModalProps) {
  const { t, i18n } = useTranslation();
  const { token } = theme.useToken();

  if (!open || !cardCode) {
    return null;
  }

  const name = t(`tarot.meaning.${cardCode}.name`);
  const orientation = isReversed
    ? t("page.history.reversed")
    : t("page.history.upright");

  const meaningKey = (section: string) =>
    `tarot.meaning.${cardCode}.${
      isReversed ? "reversed" : "upright"
    }.${section}`;

  return (
    <ResponsiveModal
      open={open}
      onClose={onClose}
      title={`${name} · ${orientation}`}
    >
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          gap: token.marginSM,
        }}
      >
        <TarotCard
          cardCode={cardCode}
          isUpright={!isReversed}
          isFlipped
          size="md"
        />
        <Tag
          color={isReversed ? "volcano" : "purple"}
          style={{
            borderRadius: token.borderRadiusSM,
            padding: "2px 10px",
            border: "none",
            fontWeight: 500,
          }}
        >
          {orientation}
        </Tag>
      </div>

      <Card style={{ textAlign: "left", marginTop: token.marginMD }}>
        {TAROT_SECTIONS.map((section) => {
          const key = meaningKey(section);
          const text = i18n.exists(key)
            ? t(key)
            : t(`tarot.placeholder.${section}`);
          return (
            <div key={section} style={{ marginBottom: token.marginMD }}>
              <Typography.Text strong>
                {t(`tarot.section.${section}`)}
              </Typography.Text>
              <Typography.Paragraph style={{ marginTop: token.marginXS }}>
                {text}
              </Typography.Paragraph>
            </div>
          );
        })}
      </Card>
    </ResponsiveModal>
  );
}
