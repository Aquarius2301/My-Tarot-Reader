/** A spread configuration as shown to admins, including disabled spreads. */
export interface AdminAiTarotReadingCountItem {
  id: string;
  cardCount: number;
  whiteCoins: number;
  positionsEn: string[];
  positionsVi: string[];
  minWords: number;
  maxWords: number;
  isEnabled: boolean;
}

/** A question type as shown to admins, including disabled types. */
export interface AdminAiTarotReadingTypeItem {
  id: string;
  code: string;
  nameEn: string;
  nameVi: string;
  isEnabled: boolean;
}

/** Result of listing every AI tarot setting for admins. */
export interface GetAllAiTarotSettingsForAdminResult {
  readingCounts: AdminAiTarotReadingCountItem[];
  readingTypes: AdminAiTarotReadingTypeItem[];
}

/** Request to create a spread configuration. */
export interface CreateAiTarotReadingCountRequest {
  cardCount: number;
  whiteCoins: number;
  positionsEn: string[];
  positionsVi: string[];
  minWords: number;
  maxWords: number;
  isEnabled: boolean;
}

/** Request to update an existing spread configuration. */
export interface UpdateAiTarotReadingCountRequest {
  cardCount: number;
  whiteCoins: number;
  positionsEn: string[];
  positionsVi: string[];
  minWords: number;
  maxWords: number;
  isEnabled: boolean;
}

/** Request to create a question type. */
export interface CreateAiTarotReadingTypeRequest {
  code: string;
  nameEn: string;
  nameVi: string;
  isEnabled: boolean;
}

/** Request to update an existing question type. */
export interface UpdateAiTarotReadingTypeRequest {
  code: string;
  nameEn: string;
  nameVi: string;
  isEnabled: boolean;
}
