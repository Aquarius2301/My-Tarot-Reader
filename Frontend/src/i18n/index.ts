import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import { viPages } from "./locales/pages/vi.pages";
import { enPages } from "./locales/pages/en.pages";
import { viErrors } from "./locales/errors/vi.errors";
import { enErrors } from "./locales/errors/en.errors";
import { viTarot } from "./locales/tarot/vi.tarot";
import { enTarot } from "./locales/tarot/en.tarot";

/**
 * Merge tarot meanings data into i18n resources under tarot.meanings
 */
function buildResources() {
  return {
    vi: {
      translation: {
        ...viPages,
        ...viErrors,
        ...viTarot,
      },
    },
    en: {
      translation: {
        ...enPages,
        ...enErrors,
        ...enTarot,
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
