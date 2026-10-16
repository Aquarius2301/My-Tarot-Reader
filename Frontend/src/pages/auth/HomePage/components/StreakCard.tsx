import { ErrorComponent, CoinIcon } from "@/components";
import { STREAK_DAILY_REWARDS, STREAK_CYCLE_DAYS } from "@/constants";
import { useCheckIn, useGetStreak } from "@/hooks/api";
import { getErrorMessage, getDaysUntilNextMonthInVietnam } from "@/utils";
import {
  App,
  Button,
  Card,
  Divider,
  Grid,
  Spin,
  Tag,
  Typography,
  theme,
} from "antd";
import {
  CheckCircleFilled,
  FireFilled,
  SafetyCertificateFilled,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";

const { Title, Text } = Typography;
const { useBreakpoint } = Grid;
const DAY_LABEL_KEYS = [
  "page.streak.day1",
  "page.streak.day2",
  "page.streak.day3",
  "page.streak.day4",
  "page.streak.day5",
  "page.streak.day6",
  "page.streak.day7",
];

export default function StreakCard() {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const { message } = App.useApp();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const { data, isLoading, refetch } = useGetStreak();
  const { mutate, isPending } = useCheckIn();

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  const {
    cycleDay,
    currentStreak,
    longestStreak,
    isCheckedInToday,
    isSaverUsed,
  } = data;

  const todayIndex = isCheckedInToday ? cycleDay - 1 : cycleDay;
  const nextIndex = Math.max(0, cycleDay);

  const handleCheckIn = () => {
    mutate(undefined, {
      onSuccess: () => message.success(t("page.streak.checkInSuccess")),
      onError: (error) => message.error(getErrorMessage(error)),
    });
  };

  return (
    <Card
      style={{
        borderRadius: token.borderRadiusLG,
        background: `linear-gradient(135deg, ${token.colorBgElevated} 0%, ${token.colorBgContainer} 100%)`,
        border: `1px solid ${token.colorBorder}`,
        boxShadow: token.boxShadowSecondary,
        marginBottom: 32,
      }}
      styles={{
        body: { padding: `${token.paddingLG}px ${token.paddingLG}px` },
      }}
    >
      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          alignItems: "center",
          justifyContent: "space-between",
          gap: token.marginMD,
          marginBottom: token.marginMD,
        }}
      >
        <div>
          <Title level={4} style={{ margin: 0, marginBottom: 4 }}>
            <FireFilled style={{ color: token.colorPrimary, marginRight: 8 }} />
            {t("page.streak.title")}
          </Title>
          <Text type="secondary" style={{ fontSize: 14 }}>
            {t("page.streak.subtitle")}
          </Text>
          <div style={{ marginTop: token.marginXS }}>
            <Tag
              icon={<SafetyCertificateFilled />}
              color={isSaverUsed ? "warning" : "success"}
              style={{ borderRadius: token.borderRadius }}
            >
              {t(
                isSaverUsed
                  ? "page.streak.saverUsed"
                  : "page.streak.saverAvailable",
              )}
            </Tag>
            {isSaverUsed && (
              <div>
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {t("page.streak.saverResetIn", {
                    days: getDaysUntilNextMonthInVietnam(),
                  })}
                </Text>
              </div>
            )}
          </div>
        </div>

        <div style={{ display: "flex", gap: token.marginLG }}>
          <Stat
            label={t("page.streak.currentStreak")}
            value={currentStreak}
            unit={t("page.streak.days")}
          />
          <Stat
            label={t("page.streak.longestStreak")}
            value={longestStreak}
            unit={t("page.streak.days")}
          />
        </div>
      </div>

      <Divider
        style={{ margin: `${token.marginSM}px 0 ${token.marginMD}px` }}
      />

      <div
        style={{
          ...(isMobile && {
            overflowX: "auto",
            overflowY: "hidden",
            WebkitOverflowScrolling: "touch",
            scrollbarWidth: "none",
            msOverflowStyle: "none",
          }),
          marginBottom: token.marginLG,
        }}
      >
        <div
          style={{
            display: "grid",
            gridTemplateColumns: `repeat(${STREAK_CYCLE_DAYS}, ${
              isMobile ? "auto" : "1fr"
            })`,
            gap: token.marginXS,
            ...(isMobile && { width: "max-content", minWidth: "100%" }),
          }}
        >
          {STREAK_DAILY_REWARDS.map((reward, i) => {
            const isToday = i === todayIndex;
            const isNext = i === nextIndex;
            const isChecked = i < todayIndex || (isCheckedInToday && isToday);

            return (
              <div
                key={i}
                style={{
                  display: "flex",
                  flexDirection: "column",
                  alignItems: "center",
                  gap: 4,
                  padding: `${token.paddingXS}px 4px`,
                  borderRadius: token.borderRadius,
                  border: `1px solid ${
                    isNext ? token.colorPrimary : token.colorBorderSecondary
                  }`,
                  background: isNext
                    ? `${token.colorPrimaryBg}`
                    : "transparent",
                }}
              >
                <Text
                  style={{
                    fontSize: 12,
                    fontWeight: isNext ? 600 : 400,
                    color: isNext
                      ? token.colorPrimary
                      : token.colorTextSecondary,
                  }}
                >
                  {t(DAY_LABEL_KEYS[i])}
                </Text>
                <span
                  style={{
                    fontSize: 16,
                    fontWeight: 600,
                    color: isChecked ? token.colorSuccess : token.colorText,
                  }}
                >
                  {isChecked ? (
                    <CheckCircleFilled />
                  ) : (
                    <span
                      style={{
                        display: "inline-flex",
                        alignItems: "center",
                        gap: 2,
                      }}
                    >
                      <CoinIcon variant="white" size={13} />
                      {reward}
                    </span>
                  )}
                </span>
              </div>
            );
          })}
        </div>
      </div>

      {!isCheckedInToday && (
        <div style={{ textAlign: "center" }}>
          <Tag
            icon={<CoinIcon variant="white" size={14} />}
            style={{
              fontSize: 14,
              padding: "4px 12px",
              marginBottom: token.marginSM,
            }}
          >
            {t("page.streak.reward")}: {STREAK_DAILY_REWARDS[nextIndex]}{" "}
            {t("component.mainLayout.whiteCoin")}
          </Tag>
          <br />
          <Button
            type="primary"
            size="large"
            icon={<FireFilled />}
            loading={isPending}
            onClick={handleCheckIn}
            style={{
              height: 44,
              padding: "0 28px",
              fontSize: 16,
              borderRadius: token.borderRadius,
              boxShadow: `0 4px 14px ${token.colorPrimary}40`,
            }}
          >
            {t("page.streak.checkIn")}
          </Button>
        </div>
      )}

      {isCheckedInToday && (
        <div style={{ textAlign: "center" }}>
          <Text type="success" style={{ fontSize: 16 }}>
            <CheckCircleFilled style={{ marginRight: 8 }} />
            {t("page.streak.checkedInToday")}
          </Text>
        </div>
      )}
    </Card>
  );
}

function Stat({
  label,
  value,
  unit,
}: {
  label: string;
  value: number;
  unit: string;
}) {
  const { token } = theme.useToken();

  return (
    <div style={{ textAlign: "center" }}>
      <Text
        style={{
          display: "block",
          fontSize: 24,
          fontWeight: 700,
          color: token.colorPrimary,
          lineHeight: 1.2,
        }}
      >
        {value} {unit}
      </Text>
      <Text type="secondary" style={{ fontSize: 12 }}>
        {label}
      </Text>
    </div>
  );
}
