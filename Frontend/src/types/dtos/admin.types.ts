/** Aggregated AI tarot reading count for one question type + card count group. */
export interface GetTarotStatisticAiTarotItem {
  questionType: string;
  cardCount: number;
  count: number;
}

/** Aggregated tarot reading statistics (guest, registered and AI tarot). */
export interface GetTarotStatisticResult {
  totalGuestTarotReadings: number;
  totalRegisteredTarotReadings: number;
  totalAiTarotReadings: number;
  aiTarotBreakdown: GetTarotStatisticAiTarotItem[];
}
