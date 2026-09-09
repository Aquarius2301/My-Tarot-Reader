import { useState } from "react";
import { Tabs, theme } from "antd";
import { useTranslation } from "react-i18next";
import {
  CardMeaningSections,
  ResponsiveModal,
  TarotCard,
  type MeaningOrientation,
} from "@/components";
import type { TarotCardCode } from "@/constants";

export interface TarotCardMeaningTabsModalProps {
  open: boolean;
  onClose: () => void;
  cardCode?: TarotCardCode;
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
  const [orientation, setOrientation] =
    useState<MeaningOrientation>("upright");

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
          onChange={(key) => setOrientation(key as MeaningOrientation)}
          items={[
            {
              key: "upright" as const,
              label: t("page.library.upright"),
              children: (
                <CardMeaningSections
                  cardCode={cardCode}
                  orientation="upright"
                  scrollable
                />
              ),
            },
            {
              key: "reversed" as const,
              label: t("page.library.reversed"),
              children: (
                <CardMeaningSections
                  cardCode={cardCode}
                  orientation="reversed"
                  scrollable
                />
              ),
            },
          ]}
        />
      </div>
    </ResponsiveModal>
  );
}