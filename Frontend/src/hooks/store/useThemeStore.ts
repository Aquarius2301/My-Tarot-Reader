import { DEFAULT_THEME_MODE, type ThemeMode } from "@/constants";
import { create } from "zustand";
import { persist } from "zustand/middleware";

interface ThemeState {
  mode: ThemeMode;
  toggle: () => void;
  setMode: (mode: ThemeMode) => void;
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set) => ({
      mode: DEFAULT_THEME_MODE,
      toggle: () =>
        set((s) => ({ mode: s.mode === "light" ? "dark" : "light" })),
      setMode: (mode) => set({ mode }),
    }),
    { name: "tarot-theme" },
  ),
);
