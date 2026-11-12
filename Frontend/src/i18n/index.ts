import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import { viPages } from "@/i18n/locales/pages/vi.pages";
import { enPages } from "@/i18n/locales/pages/en.pages";
import { viComponents } from "@/i18n/locales/components/vi.components";
import { enComponents } from "@/i18n/locales/components/en.components";
import { viTarot } from "@/i18n/locales/tarot/vi.tarot";
import { enTarot } from "@/i18n/locales/tarot/en.tarot";
import { viErrors } from "@/i18n/locales/errors/vi.errors";
import { enErrors } from "@/i18n/locales/errors/en.errors";

/**
 * Merge tarot meanings data into i18n resources under tarot.meanings
 */
function buildResources() {
  return {
    vi: {
      translation: {
        ...viPages,
        ...viComponents,
        ...viTarot,
        ...viErrors,
      },
    },
    en: {
      translation: {
        ...enPages,
        ...enComponents,
        ...enTarot,
        ...enErrors,
      },
    },
  };
}

export const resources = buildResources();

i18n.use(initReactI18next).init({
  resources,
  lng: "vi",
  fallbackLng: "en",
  interpolation: {
    escapeValue: false,
  },
});

export default i18n;
