import {
  CardMeaningSections,
  OrientationTag,
  ResponsiveModal,
  TarotCard,
} from "@/components";
import type { TarotCardCode } from "@/constants";
import { theme } from "antd";
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
  const { t } = useTranslation();
  const { token } = theme.useToken();

  if (!open || !cardCode) {
    return null;
  }

  const name = t(`tarot.meaning.${cardCode}.name`);
  const orientation = isReversed
    ? t("page.history.reversed")
    : t("page.history.upright");

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
        <OrientationTag
          isReversed={isReversed}
          style={{
            padding: "2px 10px",
            fontWeight: 500,
          }}
        >
          {orientation}
        </OrientationTag>
      </div>

      <CardMeaningSections
        cardCode={cardCode}
        orientation={isReversed ? "reversed" : "upright"}
        cardStyle={{ marginTop: token.marginMD }}
      />
    </ResponsiveModal>
  );
}
