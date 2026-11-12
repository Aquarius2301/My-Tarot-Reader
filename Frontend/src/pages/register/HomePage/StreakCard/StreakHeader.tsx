import { getDaysUntilNextMonthInVietnam } from "@/utils";
import { FireFilled, SafetyCertificateFilled } from "@ant-design/icons";
import { Flex, Tag, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { StreakStat } from "./StreakStat";

const { Title, Text } = Typography;

export type StreakHeaderProps = {
  currentStreak: number;
  longestStreak: number;
  isSaverUsed: boolean;
};

export function StreakHeader({
  currentStreak,
  longestStreak,
  isSaverUsed,
}: StreakHeaderProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  return (
    <Flex
      wrap
      justify="space-between"
      align="center"
      style={{
        gap: token.marginMD,
        marginBottom: token.marginMD,
      }}
    >
      <div>
        <Title level={4} style={{ margin: 0, marginBottom: 4 }}>
          <FireFilled style={{ color: token.colorPrimary, marginRight: 8 }} />
          {t("page.home.streakTitle")}
        </Title>
        <Text type="secondary" style={{ fontSize: 14 }}>
          {t("page.home.streakSubtitle")}
        </Text>
        <div style={{ marginTop: token.marginXS }}>
          <Tag
            icon={<SafetyCertificateFilled />}
            color={isSaverUsed ? "warning" : "success"}
            style={{ borderRadius: token.borderRadius }}
          >
            {t(
              isSaverUsed
                ? "page.home.streakSaverUsed"
                : "page.home.streakSaverAvailable",
            )}
          </Tag>
          {!isSaverUsed && (
            <Text
              type="secondary"
              style={{ fontSize: 11, marginTop: 4, display: "block" }}
            >
              {t("page.home.streakSaverDescription")}
            </Text>
          )}
          {isSaverUsed && (
            <Text
              type="secondary"
              style={{ fontSize: 12, marginTop: 4, display: "block" }}
            >
              {t("page.home.streakSaverResetIn", {
                days: getDaysUntilNextMonthInVietnam(),
              })}
            </Text>
          )}
        </div>
      </div>

      <div style={{ display: "flex", gap: token.marginLG }}>
        <StreakStat
          label={t("page.home.streakCurrentStreak")}
          value={currentStreak}
          unit={t("page.home.streakDays")}
        />
        <StreakStat
          label={t("page.home.streakLongestStreak")}
          value={longestStreak}
          unit={t("page.home.streakDays")}
        />
      </div>
    </Flex>
  );
}
