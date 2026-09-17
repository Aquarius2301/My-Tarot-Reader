export const LANGUAGE_MODE = ["en", "vi"] as const;

export type LanguageMode = (typeof LANGUAGE_MODE)[number];

export const DEFAULT_LANGUAGE_MODE: LanguageMode = "vi" as const;
