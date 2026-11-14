import { ErrorView } from "@/components";
import { STREAK_CYCLE_DAYS, STREAK_DAILY_REWARDS } from "@/constants";
import { useCheckIn, useGetStreak } from "@/hooks/api";
import { getErrorMessage } from "@/utils";
import { App, Card, Divider } from "antd";
import { useTranslation } from "react-i18next";
import { StreakCheckIn } from "./StreakCheckIn";
import { StreakDayGrid } from "./StreakDayGrid";
import { StreakHeader } from "./StreakHeader";

export function StreakCard() {
  const { t } = useTranslation();
  const { message } = App.useApp();

  const { data, isLoading, refetch } = useGetStreak();
  const { mutate, isPending } = useCheckIn();

  if (isLoading) {
    return <Card loading />;
  }

  if (data === undefined) {
    return <ErrorView type="server" onRetry={refetch} />;
  }

  const {
    cycleDay,
    currentStreak,
    longestStreak,
    isCheckedInToday,
    isSaverUsed,
  } = data;

  const checkedCount = Math.min(currentStreak, STREAK_CYCLE_DAYS);
  const isNextIndex = cycleDay - 1;
  const nextReward =
    STREAK_DAILY_REWARDS[isNextIndex] ?? STREAK_DAILY_REWARDS[0];

  const handleCheckIn = () => {
    mutate(undefined, {
      onSuccess: () => message.success(t("page.home.streakCheckInSuccess")),
      onError: (error) => message.error(getErrorMessage(error)),
    });
  };

  return (
    <Card>
      <StreakHeader
        currentStreak={currentStreak}
        longestStreak={longestStreak}
        isSaverUsed={isSaverUsed}
      />

      <Divider style={{}} />

      <StreakDayGrid
        checkedCount={checkedCount}
        isNextIndex={isNextIndex}
        isCheckedInToday={isCheckedInToday}
      />

      <StreakCheckIn
        isCheckedInToday={isCheckedInToday}
        nextReward={nextReward}
        isPending={isPending}
        onCheckIn={handleCheckIn}
      />
    </Card>
  );
}
