import { Card, Typography } from "antd";
import { TAROT_SECTIONS, type TarotCardCode } from "@/constants";
import { useTranslation } from "react-i18next";
import i18n from "@/i18n";

interface TarotMeaningCardProps {
  cardCode: TarotCardCode;
  isReversed: boolean;
}

export default function TarotMeaningCard({
  cardCode,
  isReversed,
}: TarotMeaningCardProps) {
  const { t } = useTranslation();

  const meaningKey = (section: string) =>
    `tarot.meaning.${cardCode}.${isReversed ? "reversed" : "upright"}.${section}`;

  return (
    <Card style={{ textAlign: "left", marginTop: 16 }}>
      {TAROT_SECTIONS.map((section) => {
        const key = meaningKey(section);
        const text = i18n.exists(key) ? t(key) : t(`tarot.placeholder.${section}`);
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
  );
}