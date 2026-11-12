import type { ReactNode } from "react";
import { theme as antdTheme, type ThemeConfig } from "antd";

export type ThemeMode = "light" | "dark";

export const DEFAULT_THEME_MODE: ThemeMode = "light" as const;

export type LayoutVariant = "guest" | "register" | "admin";

export type LayoutMenuItem = {
  key: string;
  label: string;
  icon?: ReactNode;
  children?: LayoutMenuItem[];
};

export type Palette = {
  primary: string;
  bgBase: string;
  siderBg: string;
  accentGlow: string;
};

export const layoutPalettes: Record<
  LayoutVariant,
  Record<ThemeMode, Palette>
> = {
  guest: {
    light: {
      primary: "#0284c7",
      bgBase: "#f8fafc",
      siderBg: "#ffffff",
      accentGlow: "rgba(56, 189, 248, 0.25)",
    },
    dark: {
      primary: "#38bdf8",
      bgBase: "#0f172a",
      siderBg: "#090d16",
      accentGlow: "rgba(56, 189, 248, 0.25)",
    },
  },
  register: {
    light: {
      primary: "#7e22ce",
      bgBase: "#faf5ff",
      siderBg: "#ffffff",
      accentGlow: "rgba(168, 85, 247, 0.25)",
    },
    dark: {
      primary: "#a855f7",
      bgBase: "#0b0f19",
      siderBg: "#070a12",
      accentGlow: "rgba(168, 85, 247, 0.25)",
    },
  },
  admin: {
    light: {
      primary: "#059669",
      bgBase: "#ecfdf5",
      siderBg: "#ffffff",
      accentGlow: "rgba(16, 185, 129, 0.25)",
    },
    dark: {
      primary: "#10b981",
      bgBase: "#022c22",
      siderBg: "#011c15",
      accentGlow: "rgba(16, 185, 129, 0.25)",
    },
  },
};

export const coinColors = {
  /** White coin on dark surfaces: bright, near-white. */
  white: "#F2EFFF",
  /** White coin on light surfaces: silver-grey so it stays visible against a light header. */
  whiteLight: "#6E6A85",
  red: "#e84f54",
  redDark: "#dd2026",
} as const;

export function getPalette(variant: LayoutVariant, mode: ThemeMode): Palette {
  return layoutPalettes[variant][mode];
}

export const getTheme = (isDark: boolean, palette: Palette): ThemeConfig => {
  return {
    algorithm: isDark ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
    token: {
      colorPrimary: palette.primary,
      colorBgLayout: palette.bgBase,
      colorBgContainer: palette.siderBg,
      colorTextBase: isDark ? "#f8fafc" : "#0f172a",
      borderRadius: 10,
      fontFamily:
        "'Plus Jakarta Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
    },
    components: {
      Layout: {
        siderBg: palette.siderBg,
        headerBg: palette.siderBg,
        bodyBg: palette.bgBase,
        triggerBg: palette.siderBg,
      },
      Menu: {
        itemBg: "transparent",
        itemSelectedBg: palette.accentGlow,
        itemSelectedColor: palette.primary,
        itemHoverBg: palette.accentGlow,
        itemBorderRadius: 10,
      },
      Card: {
        colorBorderSecondary: palette.accentGlow,
      },
      Divider: {
        colorSplit: palette.accentGlow,
      },
    },
  };
};
