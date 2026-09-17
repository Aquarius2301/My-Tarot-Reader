import { Button, Space, Typography } from "antd";
import Text from "antd/es/typography/Text";
import { useTranslation } from "react-i18next";

export interface TarotSpreadControlsProps {
  selectedCount: number;
  limit: number;
  busy: boolean;
  isFull: boolean;
  onReshuffle: () => void;
  onConfirm: () => void;
  disabledConfirm?: boolean;
}

/** Action row below the spread: selection count, reshuffle, and confirm. */
export default function TarotSpreadControls({
  selectedCount,
  limit,
  busy,
  isFull,
  onReshuffle,
  onConfirm,
  disabledConfirm,
}: TarotSpreadControlsProps) {
  const { t } = useTranslation();

  return (
    <Space style={{ marginTop: 44 }}>
      <Typography.Text>
        {t("component.deck.selected", { count: selectedCount, limit })}
      </Typography.Text>
      <Button onClick={onReshuffle} disabled={busy}>
        {t("component.deck.reshuffle")}
      </Button>

      <Button
        type="primary"
        disabled={!isFull || busy || disabledConfirm}
        onClick={onConfirm}
      >
        {t("component.deck.confirm")}
      </Button>

      {disabledConfirm && (
        <Text type="danger" style={{ marginLeft: 12 }}>
          {t("component.deck.noEnoughCoin")}
        </Text>
      )}
    </Space>
  );
}
