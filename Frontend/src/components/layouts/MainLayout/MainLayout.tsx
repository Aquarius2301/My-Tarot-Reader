import { useState, type ReactNode } from "react";
import {
  App as AntdApp,
  ConfigProvider,
  Layout,
  Typography,
  Grid,
  type MenuProps,
  Spin,
} from "antd";
import { LogoutOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";

import { useThemeStore, useLanguageStore } from "@/hooks/stores";
import type { GetCurrentUserResult, UserRole } from "@/types";
import { useLogout } from "@/hooks/api";
import { useNavigate } from "react-router-dom";
import { LayoutHeader, LayoutFooter, MobileDrawer } from "./components";
import type { Palette } from "./components/LayoutFooter";
import { getPaletteByRole, getThemeByRole } from "@/constants";
import { WEB_URL } from "@/routes";

export interface MainLayoutProps {
  role?: UserRole;
  children: ReactNode;
  user?: GetCurrentUserResult;
  currentPath?: string;
}

export interface NavItem {
  key: string;
  label: string;
  href?: string;
  children?: NavItem[];
}

const { Content } = Layout;
const { useBreakpoint } = Grid;
const { Text } = Typography;

export default function MainLayout({
  role,
  children,
  user,
  currentPath = typeof window !== "undefined" ? window.location.pathname : "/",
}: MainLayoutProps) {
  const screens = useBreakpoint();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const { t } = useTranslation();

  const themeMode = useThemeStore((s) => s.mode);
  const setThemeMode = useThemeStore((s) => s.setMode);

  const langMode = useLanguageStore((s) => s.mode);
  const setLangMode = useLanguageStore((s) => s.setMode);

  const themeConfig = getThemeByRole(role, themeMode);
  const palette: Palette = getPaletteByRole(role, themeMode);

  const navigate = useNavigate();
  const { mutate, isPending } = useLogout();
  const isMobile = !screens.md;

  const toggleTheme = () => {
    setThemeMode(themeMode === "dark" ? "light" : "dark");
  };

  const toggleLanguage = () => {
    setLangMode(langMode === "vi" ? "en" : "vi");
  };

  const NAV_ITEMS: NavItem[] = [
    {
      key: "home",
      label: t("page.home.title"),
      href: user ? WEB_URL.home : WEB_URL.guestHome,
    },
    {
      key: "tarot",
      label: t("page.tarot.parentTitle"),
      children: [
        {
          key: "tarot2",
          label: t("page.tarot.title"),
          href: user ? WEB_URL.tarot : WEB_URL.guestTarot,
        },
        user && {
          key: "aiTarot",
          label: t("page.aiTarot.title"),
          href: WEB_URL.aiTarot,
        },
      ].filter(Boolean) as NavItem[],
    },
    user && {
      key: "history",
      label: t("page.history.parentTitle"),
      children: [
        user && {
          key: "history",
          label: t("page.history.title"),
          href: WEB_URL.history,
        },
        user && {
          key: "aiTarotHistory",
          label: t("page.historyAiTarot.title"),
          href: WEB_URL.aiTarotHistory,
        },
      ],
    },

    user && {
      key: "library",
      label: t("page.library.title"),
      href: WEB_URL.library,
    },
  ].filter(Boolean) as NavItem[];

  // Dropdown for user info and logout
  const userDropdownItems: MenuProps["items"] = [
    {
      key: "user-info",
      label: (
        <div style={{ padding: "4px 8px" }}>
          <Text strong style={{ display: "block" }}>
            {user?.fullName || t("component.mainLayout.guest")}
          </Text>
          <Text type="secondary" style={{ fontSize: "12px" }}>
            {user?.email}
          </Text>
        </div>
      ),
      disabled: true,
    },
    { type: "divider" },
    {
      key: "logout",
      danger: true,
      icon: <LogoutOutlined />,
      label: t("component.mainLayout.logout"),
      onClick: () => {
        mutate(undefined, {
          onSuccess: () => navigate(WEB_URL.guestHome, { replace: true }),
        });
      },
    },
  ];

  const buildMenuItems = (items: NavItem[]): MenuProps["items"] => {
    return items.map((item) => {
      if (item.children && item.children.length > 0) {
        return {
          key: item.key,
          label: item.label,
          children: item.children && buildMenuItems(item.children),
        };
      }

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
  };

  const menuItems = buildMenuItems(NAV_ITEMS);

  const handleLogin = () => navigate(WEB_URL.login);

  const handleDrawerLogout = () => {
    setMobileMenuOpen(false);
    mutate(undefined, {
      onSuccess: () => navigate(WEB_URL.guestHome, { replace: true }),
    });
  };

  return (
    <ConfigProvider theme={themeConfig}>
      <AntdApp>
        <Layout
          style={{ minHeight: "100vh", backgroundColor: palette.bgLight }}
        >
          {isPending && <Spin fullscreen />}

          {/* HEADER */}
          <LayoutHeader
            isMobile={isMobile}
            user={user}
            palette={palette}
            themeMode={themeMode}
            langMode={langMode}
            currentPath={currentPath}
            menuItems={menuItems}
            userDropdownItems={userDropdownItems}
            onToggleTheme={toggleTheme}
            onToggleLanguage={toggleLanguage}
            onOpenMobileMenu={() => setMobileMenuOpen(true)}
            onLogin={handleLogin}
          />

          {/* CONTENT */}
          <Content
            style={{
              padding: isMobile ? "16px" : "24px 48px",
              color: palette.text,
            }}
          >
            {children}
          </Content>

          {/* FOOTER */}
          <LayoutFooter palette={palette} />

          {/* MOBILE DRAWER */}
          {isMobile && (
            <MobileDrawer
              open={mobileMenuOpen}
              onClose={() => setMobileMenuOpen(false)}
              user={user}
              palette={palette}
              themeMode={themeMode}
              langMode={langMode}
              currentPath={currentPath}
              menuItems={menuItems}
              onToggleTheme={toggleTheme}
              onToggleLanguage={toggleLanguage}
              onLogout={handleDrawerLogout}
              onLogin={handleLogin}
            />
          )}
        </Layout>
      </AntdApp>
    </ConfigProvider>
  );
}
