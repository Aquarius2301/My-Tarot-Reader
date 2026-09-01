/**
 * The available language modes for the application.
 * This constant defines the supported languages, which can be used for localization and internationalization purposes.
 */
export const LANGUAGE_MODES = ["en", "vi"] as const;

/**
 * The type representing the language modes in the application.
 * It is derived from the `LANGUAGE_MODES` constant and can be used for type checking and ensuring that only valid language modes are assigned.
 */
export type LanguageMode = (typeof LANGUAGE_MODES)[keyof typeof LANGUAGE_MODES];

/**
 * The default language mode for the application.
 * This constant defines the initial language setting when the application is first loaded or when no user preference is set.
 */
export const DEFAULT_LANGUAGE_MODE = "vi";
