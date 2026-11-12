import { Coin } from "@/components";
import { STREAK_CYCLE_DAYS, STREAK_DAILY_REWARDS } from "@/constants";
import { CheckCircleFilled } from "@ant-design/icons";
import { Grid, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { DAY_LABEL_KEYS } from "./constants";

const { Text } = Typography;

export type StreakDayGridProps = {
  checkedCount: number;
  isNextIndex: number;
  isCheckedInToday: boolean;
};

export function StreakDayGrid({
  checkedCount,
  isNextIndex,
  isCheckedInToday,
}: StreakDayGridProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const screens = Grid.useBreakpoint();
  const isMobile = !screens.md;

  return (
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
          const isChecked = i < checkedCount;
          const isNext = !isCheckedInToday && i === isNextIndex;

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
                background: isNext ? `${token.colorPrimaryBg}` : "transparent",
              }}
            >
              <Text
                style={{
                  fontSize: 12,
                  fontWeight: isNext ? 600 : 400,
                  color: isNext ? token.colorPrimary : token.colorTextSecondary,
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
                    <Coin type="white" number={reward} isTooltip={false} />
                  </span>
                )}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}
