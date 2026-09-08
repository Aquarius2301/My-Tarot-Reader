import { useState } from "react";
import { Card, Tabs, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { ResponsiveModal, TarotCard } from "@/components";
import { TAROT_SECTIONS, type TarotCardCode } from "@/constants";

type Orientation = "upright" | "reversed";

export interface TarotCardMeaningTabsModalProps {
  open: boolean;
  onClose: () => void;
  cardCode?: TarotCardCode;
}

/** The five meaning sections for one card in one orientation, scrollable so
 * the card face and orientation tabs above stay fixed. */
function MeaningPanel({
  cardCode,
  orientation,
}: {
  cardCode: TarotCardCode;
  orientation: Orientation;
}) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  return (
    <div style={{ overflow: "auto", maxHeight: "45vh" }}>
      <Card style={{ textAlign: "left" }}>
        {TAROT_SECTIONS.map((section) => {
          const key = `tarot.meaning.${cardCode}.${orientation}.${section}`;
          return (
            <div key={section} style={{ marginBottom: token.marginMD }}>
              <Typography.Text strong>
                {t(`tarot.section.${section}`)}
              </Typography.Text>
              <Typography.Paragraph style={{ marginTop: token.marginXS }}>
                {t(key, {
                  defaultValue: t(`tarot.placeholder.${section}`),
                })}
              </Typography.Paragraph>
            </div>
          );
        })}
      </Card>
    </div>
  );
}

/**
 * Card-meaning modal with an Upright/Reversed tab switcher. Each tab is a real
 * panel so the aria relationship (and focus order) is correct; the card face and
 * the tab bar stay fixed while the meaning sections scroll inside their own
 * region on the mobile bottom-drawer (capped at 85vh by ResponsiveModal).
 */
export default function TarotCardMeaningTabsModal({
  open,
  onClose,
  cardCode,
}: TarotCardMeaningTabsModalProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const [orientation, setOrientation] = useState<Orientation>("upright");

  if (!open || !cardCode) {
    return null;
  }

  const name = t(`tarot.meaning.${cardCode}.name`);

  return (
    <ResponsiveModal open={open} onClose={onClose} title={name}>
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
          isUpright={orientation === "upright"}
          isFlipped
          size="md"
        />
        <Tabs
          centered
          activeKey={orientation}
          onChange={(key) => setOrientation(key as Orientation)}
          items={[
            {
              key: "upright" as const,
              label: t("page.library.upright"),
              children: (
                <MeaningPanel cardCode={cardCode} orientation="upright" />
              ),
            },
            {
              key: "reversed" as const,
              label: t("page.library.reversed"),
              children: (
                <MeaningPanel cardCode={cardCode} orientation="reversed" />
              ),
            },
          ]}
        />
      </div>
    </ResponsiveModal>
  );
}