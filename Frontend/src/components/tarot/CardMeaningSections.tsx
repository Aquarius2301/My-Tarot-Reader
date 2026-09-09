import type { CSSProperties } from "react";
import { Card, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { TAROT_SECTIONS, type TarotCardCode } from "@/constants";

export type MeaningOrientation = "upright" | "reversed";

export interface CardMeaningSectionsProps {
  cardCode: TarotCardCode;
  orientation: MeaningOrientation;
  /** When true, wrap the card in a vertically scrollable region (used inside tabs). */
  scrollable?: boolean;
  /** Extra style merged onto the wrapping Card (e.g. margin). */
  cardStyle?: CSSProperties;
}

/**
 * The five meaning sections (keywords, description, ...) of a single card in
 * one orientation. Shared by the history single-card modal and the library
 * upright/reversed tabs modal so the meaning layout stays identical.
 */
export default function CardMeaningSections({
  cardCode,
  orientation,
  scrollable = false,
  cardStyle,
}: CardMeaningSectionsProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const card = (
    <Card style={{ textAlign: "left", ...cardStyle }}>
      {TAROT_SECTIONS.map((section) => {
        const key = `tarot.meaning.${cardCode}.${orientation}.${section}`;
        return (
          <div key={section} style={{ marginBottom: token.marginMD }}>
            <Typography.Text strong>
              {t(`tarot.section.${section}`)}
            </Typography.Text>
            <Typography.Paragraph style={{ marginTop: token.marginXS }}>
              {t(key, { defaultValue: t(`tarot.placeholder.${section}`) })}
            </Typography.Paragraph>
          </div>
        );
      })}
    </Card>
  );

  return scrollable ? (
    <div style={{ overflow: "auto", maxHeight: "45vh" }}>{card}</div>
  ) : (
    card
  );
}
