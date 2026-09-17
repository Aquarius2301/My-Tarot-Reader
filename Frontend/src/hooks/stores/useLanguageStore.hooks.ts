import { DEFAULT_LANGUAGE_MODE, type LanguageMode } from "@/constants";
import i18n from "@/i18n";
import { create } from "zustand";
import { persist } from "zustand/middleware";

interface LanguageState {
  mode: LanguageMode;
  toggle: () => void;
  setMode: (mode: LanguageMode) => void;
}

export const useLanguageStore = create<LanguageState>()(
  persist(
    (set) => ({
      mode: DEFAULT_LANGUAGE_MODE,
      toggle: () =>
        set((s) => {
          const next: LanguageMode = s.mode === "en" ? "vi" : "en";
          i18n.changeLanguage(String(next));
          return { mode: next };
        }),
      setMode: (mode) => {
        i18n.changeLanguage(String(mode));
        set({ mode });
      },
    }),
    { name: "tarot-language" },
  ),
);

// Sync i18n with the persisted language on load so a previously chosen
// language (e.g. "en") is reflected before the first render.
i18n.changeLanguage(String(useLanguageStore.getState().mode));
