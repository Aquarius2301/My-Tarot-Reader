import { APP_NAME, getPalette, getTheme } from "@/constants";
import { useThemeStore } from "@/hooks/stores";
import { ConfigProvider, Spin } from "antd";
import { useLocation } from "react-router-dom";
import type { AppLoadingProps } from "./types";
import { getVariantForPath } from "./utils";

/**
 * Themed full-screen splash shown while the app boots or a route is resolving.
 *
 * It reads the persisted theme mode itself and wraps the content in its own
 * ConfigProvider, because it renders before any layout (and therefore before
 * the layout's theme) is mounted.
 */
export function AppLoading({ variant, description }: AppLoadingProps) {
  const mode = useThemeStore((state) => state.mode);
  const location = useLocation();

  const isDark = mode === "dark";
  const resolvedVariant = variant ?? getVariantForPath(location.pathname);
  const palette = getPalette(resolvedVariant, mode);

  return (
    <ConfigProvider theme={getTheme(isDark, palette)}>
      <div
        className="app-loading"
        style={{ background: palette.bgBase, colorScheme: mode }}
        role="status"
        aria-live="polite"
        aria-label={description ?? APP_NAME}
      >
        <div className="app-loading__brand">
          <img className="app-loading__logo" src="/logo.png" alt={APP_NAME} />
          <span
            className="app-loading__name"
            style={{ color: palette.primary }}
          >
            {APP_NAME}
          </span>
        </div>

        <Spin size="large" />

        {description && (
          <span className="app-loading__description">{description}</span>
        )}
      </div>
    </ConfigProvider>
  );
}
