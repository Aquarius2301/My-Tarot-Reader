import { type ThemeConfig, theme } from "antd";
import type { UserRole } from "@/constants";

/**
 * The available theme modes for the application.
 */
export const THEME_MODE = ["dark", "light"] as const;

/**
 * The type representing the theme modes in the application.
 * It is derived from the `THEME_MODE` constant and can be used for type checking and ensuring that only valid theme modes are assigned.
 */
export type ThemeMode = (typeof THEME_MODE)[number];

/**
 * The default theme mode for the application.
 */
export const DEFAULT_THEME_MODE = "dark";

// Color palettes for each user role and mode, including primary, hover, active, glow, textOnPrimary, and surface colors.
export const COLOR_PALETTES = {
  guest: {
    primary: "#6B5CA5",
    hover: "#8574C4",
    active: "#574A8A",
    glow: "#B8A9E8",
    textOnPrimary: "#F5F0FF",
    surface: {
      dark: { bg: "#0F0B1E", border: "#2A2145" },
      light: { bg: "#F1EDF9", border: "#D9CFF0" },
    },
  },
  registered: {
    primary: "#9D5CE0",
    hover: "#B47AF0",
    active: "#7C3FBF",
    glow: "#D4B8FF",
    textOnPrimary: "#FBF6FF",
    surface: {
      dark: { bg: "#150F28", border: "#3A2A5C" },
      light: { bg: "#F5EEFC", border: "#DFCCF5" },
    },
  },
  pro: {
    primary: "#D4AF37",
    hover: "#E8C766",
    active: "#B8942C",
    glow: "#F5DFA0",
    textOnPrimary: "#241C08",
    surface: {
      dark: { bg: "#1A1509", border: "#4A3D1A" },
      light: { bg: "#FBF6E8", border: "#E8D9A8" },
    },
  },
} as const;

// Base surface colors for each mode
const BASE_SURFACE: Record<
  ThemeMode,
  {
    bgContainer: string;
    bgElevated: string;
    text: string;
    textSecondary: string;
  }
> = {
  dark: {
    bgContainer: "#17122B",
    bgElevated: "#1C1735",
    text: "#EDE6FF",
    textSecondary: "#B3A6D9",
  },
  light: {
    bgContainer: "#FFFDF8",
    bgElevated: "#FFFFFF",
    text: "#2A2145",
    textSecondary: "#7A6B9E",
  },
};

/** Body font */
const FONT_BODY = "'Cormorant Garamond', 'EB Garamond', Georgia, serif";

const buildTheme = (
  role: keyof typeof COLOR_PALETTES,
  mode: ThemeMode,
): ThemeConfig => {
  const p = COLOR_PALETTES[role];
  const surface = p.surface[mode];
  const base = BASE_SURFACE[mode];
  return {
    algorithm: mode === "dark" ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      colorPrimary: p.primary,
      colorPrimaryHover: p.hover,
      colorPrimaryActive: p.active,
      colorBgBase: surface.bg,
      colorBgContainer: base.bgContainer,
      colorBgElevated: base.bgElevated,
      colorBorder: surface.border,
      colorBorderSecondary: surface.border,
      colorText: base.text,
      colorTextSecondary: base.textSecondary,
      fontFamily: FONT_BODY,
      borderRadius: 10,
      borderRadiusLG: 16,
    },
    components: {
      Button: {
        colorPrimary: p.primary,
        algorithm: true,
        borderRadius: 8,
      },
      Card: {
        colorBorderSecondary: surface.border,
        colorBgContainer: base.bgContainer,
      },
      Tag: {
        colorBgContainer: surface.bg,
      },
    },
  };
};

/**
 * Returns the Ant Design theme configuration for the given user role and mode.
 * If the role is not provided or unrecognized, it defaults to "guest".
 * If the mode is not provided, it defaults to "dark".
 */
export const getThemeByRole = (
  role?: UserRole,
  mode: ThemeMode = "dark",
): ThemeConfig => {
  const r = role ?? "guest";
  const key =
    r in COLOR_PALETTES ? (r as keyof typeof COLOR_PALETTES) : "guest";
  return buildTheme(key, mode);
};

/**
 * Returns a palette of colors for the given user role and mode.
 * If the role is not provided or unrecognized, it defaults to "guest".
 * If the mode is not provided, it defaults to "dark".
 * The returned palette includes primary, hover, active, glow, textOnPrimary,
 * bgLight, border, text, and textSecondary colors.
 */
export const getPaletteByRole = (role?: UserRole, mode: ThemeMode = "dark") => {
  const r = role ?? "guest";
  const key =
    r in COLOR_PALETTES ? (r as keyof typeof COLOR_PALETTES) : "guest";
  const p = COLOR_PALETTES[key];
  const surface = p.surface[mode];
  const base = BASE_SURFACE[mode];
  return {
    primary: p.primary,
    hover: p.hover,
    active: p.active,
    glow: p.glow,
    textOnPrimary: p.textOnPrimary,
    bgLight: surface.bg,
    border: surface.border,
    text: base.text,
    textSecondary: base.textSecondary,
  };
};
