import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import { viPages } from "@/i18n/locales/pages/vi.pages";
import { enPages } from "@/i18n/locales/pages/en.pages";
import { viComponents } from "@/i18n/locales/components/vi.components";
import { enComponents } from "@/i18n/locales/components/en.components";
import { viErrors } from "@/i18n/locales/errors/vi.errors";
import { enErrors } from "@/i18n/locales/errors/en.errors";
import { DEFAULT_LANGUAGE_MODE, type LanguageMode } from "@/constants";

/**
 * Merge tarot meanings data into i18n resources under tarot.meanings
 */
function buildResources() {
  return {
    vi: {
      translation: {
        ...viPages,
        ...viComponents,
        ...viErrors,
      },
    },
    en: {
      translation: {
        ...enPages,
        ...enComponents,
        ...enErrors,
      },
    },
  };
}

export const resources = buildResources();

i18n.use(initReactI18next).init({
  resources,
  lng: DEFAULT_LANGUAGE_MODE,
  fallbackLng: "en",
  interpolation: {
    escapeValue: false,
  },
  react: { bindI18nStore: "added" },
});

const tarotLoaders: Record<
  LanguageMode,
  () => Promise<Record<string, unknown>>
> = {
  vi: () => import("@/i18n/locales/tarot/vi.tarot").then((m) => m.viTarot),
  en: () => import("@/i18n/locales/tarot/en.tarot").then((m) => m.enTarot),
};

const loadedTarot = new Set<LanguageMode>();

/** Load and merge tarot meanings of one language into i18n resources */
export async function loadTarotLocale(lng: LanguageMode): Promise<void> {
  if (loadedTarot.has(lng)) return;
  const tarot = await tarotLoaders[lng]();
  i18n.addResourceBundle(lng, "translation", tarot, true, true);
  loadedTarot.add(lng);
}

/** Use this instead of i18n.changeLanguage so tarot data is ready first */
export async function setLanguage(lng: LanguageMode): Promise<void> {
  await loadTarotLocale(lng);
  await i18n.changeLanguage(lng);
}

export default i18n;
