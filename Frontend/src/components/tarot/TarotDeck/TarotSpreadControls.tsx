import { Button, Space, Typography } from "antd";
import { useTranslation } from "react-i18next";

export interface TarotSpreadControlsProps {
  selectedCount: number;
  limit: number;
  busy: boolean;
  isFull: boolean;
  onReshuffle: () => void;
  onConfirm: () => void;
}

/** Action row below the spread: selection count, reshuffle, and confirm. */
export default function TarotSpreadControls({
  selectedCount,
  limit,
  busy,
  isFull,
  onReshuffle,
  onConfirm,
}: TarotSpreadControlsProps) {
  const { t } = useTranslation();

  return (
    <Space style={{ marginTop: 44 }}>
      <Typography.Text>
        {t("tarot.deck.selected", { count: selectedCount, limit })}
      </Typography.Text>
      <Button onClick={onReshuffle} disabled={busy}>
        {t("tarot.deck.reshuffle")}
      </Button>
      <Button type="primary" disabled={!isFull || busy} onClick={onConfirm}>
        {t("tarot.deck.confirm")}
      </Button>
    </Space>
  );
}
