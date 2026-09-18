export interface GetStreakResult {
  cycleDay: number;
  currentStreak: number;
  longestStreak: number;
  isSaverUsed: boolean;
  isCheckedInToday: boolean;
}