/**
 * Number of consecutive check-in days that complete one cycle.
 * Must match the backend `Streak:CycleDays` setting.
 */
export const STREAK_CYCLE_DAYS = 7 as const;

/**
 * White coins rewarded per consecutive check-in day, indexed by
 * `(CurrentStreak - 1) % DailyCheckInRewards.Count` so the schedule repeats
 * every cycle. Must match the backend `Streak:DailyCheckInRewards` setting.
 */
export const STREAK_DAILY_REWARDS = [1, 1, 1, 1, 2, 2, 3] as const;