import { type ReactNode } from "react";
import {
  App as AntdApp,
  ConfigProvider,
  Grid,
  Layout,
  Spin,
  type MenuProps,
} from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";

import { useThemeStore, useLanguageStore } from "@/hooks/stores";
import type { GetCurrentUserResult, UserRole } from "@/types";
import { useLogout } from "@/hooks/api";
import { AdminHeader } from "./components";
import { getPaletteByRole, getThemeByRole } from "@/constants";
import { WEB_URL } from "@/routes";

export interface AdminLayoutProps {
  role?: UserRole;
  children: ReactNode;
  user?: GetCurrentUserResult;
  currentPath?: string;
}

export interface AdminNavItem {
  key: string;
  label: string;
  href?: string;
}

const { Content, Footer } = Layout;
const { useBreakpoint } = Grid;

export default function AdminLayout({
  role,
  children,
  user,
  currentPath =
    typeof window !== "undefined" ? window.location.pathname : "/admin",
}: AdminLayoutProps) {
  const screens = useBreakpoint();
  const { t } = useTranslation();

  const themeMode = useThemeStore((s) => s.mode);
  const setThemeMode = useThemeStore((s) => s.setMode);

  const langMode = useLanguageStore((s) => s.mode);
  const setLangMode = useLanguageStore((s) => s.setMode);

  const themeConfig = getThemeByRole(role, themeMode);
  const palette = getPaletteByRole(role, themeMode);

  const navigate = useNavigate();
  const { mutate, isPending } = useLogout();
  const isMobile = !screens.md;

  const toggleTheme = () => setThemeMode(themeMode === "dark" ? "light" : "dark");

  const toggleLanguage = () =>
    setLangMode(langMode === "vi" ? "en" : "vi");

  const ADMIN_NAV_ITEMS: AdminNavItem[] = [
    {
      key: "statistics",
      label: t("page.admin.statistics.title"),
      href: WEB_URL.adminStatistics,
    },
    {
      key: "shopPackages",
      label: t("page.admin.shopPackages.title"),
      href: WEB_URL.adminShopPackages,
    },
    {
      key: "aiTarotSettings",
      label: t("page.admin.aiTarotSettings.title"),
      href: WEB_URL.adminAiTarotSettings,
    },
  ];

  const buildMenuItems = (items: AdminNavItem[]): MenuProps["items"] =>
    items.map((item) => {
      const isCurrent = item.href === currentPath;

      return {
        key: item.href || item.key,
        disabled: isCurrent,
        label: isCurrent ? (
          <span
            style={{
              fontWeight: 700,
              color: palette.primary,
              borderBottom: `2px solid ${palette.primary}`,
              paddingBottom: "2px",
              textShadow:
                themeMode === "dark" ? `0 0 10px ${palette.glow}` : "none",
              cursor: "default",
            }}
          >
            {item.label}
          </span>
        ) : (
          <a
            onClick={(e) => {
              e.preventDefault();
              navigate(item.href ?? item.key);
            }}
          >
            {item.label}
          </a>
        ),
      };
    });

  const menuItems = buildMenuItems(ADMIN_NAV_ITEMS);

  const handleLogout = () =>
    mutate(undefined, {
      onSuccess: () => navigate(WEB_URL.guestHome, { replace: true }),
    });

  return (
    <ConfigProvider theme={themeConfig}>
      <AntdApp>
        <Layout
          style={{ minHeight: "100vh", backgroundColor: palette.bgLight }}
        >
          {isPending && <Spin fullscreen />}

          <AdminHeader
            isMobile={isMobile}
            user={user}
            palette={palette}
            themeMode={themeMode}
            langMode={langMode}
            currentPath={currentPath}
            menuItems={menuItems}
            onToggleTheme={toggleTheme}
            onToggleLanguage={toggleLanguage}
            onLogout={handleLogout}
          />

          <Content
            style={{
              padding: isMobile ? "16px" : "24px 48px",
              color: palette.text,
            }}
          >
            {children}
          </Content>

          <Footer
            style={{
              textAlign: "center",
              backgroundColor: palette.bgLight,
              borderTop: `1px solid ${palette.border}`,
              color: palette.textSecondary,
            }}
          >
            {t("page.admin.title")} · My Tarot Reader ©
            {new Date().getFullYear()}
          </Footer>
        </Layout>
      </AntdApp>
    </ConfigProvider>
  );
}
