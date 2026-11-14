import { Coin } from "@/components";
import { CheckCircleFilled, FireFilled } from "@ant-design/icons";
import { Button, Tag, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";

const { Text } = Typography;

export type StreakCheckInProps = {
  isCheckedInToday: boolean;
  nextReward: number;
  isPending: boolean;
  onCheckIn: () => void;
};

export function StreakCheckIn({
  isCheckedInToday,
  nextReward,
  isPending,
  onCheckIn,
}: StreakCheckInProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  if (isCheckedInToday) {
    return (
      <div style={{ textAlign: "center" }}>
        <Text type="success" style={{ fontSize: 16 }}>
          <CheckCircleFilled style={{ marginRight: 8 }} />
          {t("page.home.streakCheckedInToday")}
        </Text>
      </div>
    );
  }

  return (
    <div style={{ textAlign: "center" }}>
      <Tag
        style={{
          fontSize: 14,
          padding: "4px 12px",
          marginBottom: token.marginSM,
        }}
      >
        {t("page.home.streakReward")}:{" "}
        <Coin type="white" number={nextReward} isTooltip={false} />
      </Tag>
      <br />
      <Button
        type="primary"
        size="large"
        icon={<FireFilled />}
        loading={isPending}
        onClick={onCheckIn}
        style={{
          height: 44,
          padding: "0 28px",
          fontSize: 16,
          borderRadius: token.borderRadius,
          boxShadow: `0 4px 14px ${token.colorPrimary}40`,
        }}
      >
        {t("page.home.streakCheckIn")}
      </Button>
    </div>
  );
}
