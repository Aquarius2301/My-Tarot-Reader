import { Drawer, Menu, Button, Typography, type MenuProps } from "antd";
import {
  SunOutlined,
  MoonOutlined,
  GlobalOutlined,
  LogoutOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { APP_NAME, type LanguageMode } from "@/constants";
import type { UserResponse } from "@/types";
import type { Palette } from "./LayoutFooter";

const { Text } = Typography;

export interface MobileDrawerProps {
  open: boolean;
  onClose: () => void;
  user?: UserResponse;
  palette: Palette;
  themeMode: "dark" | "light";
  langMode: LanguageMode;
  currentPath: string;
  menuItems: MenuProps["items"];
  onToggleTheme: () => void;
  onToggleLanguage: () => void;
  onLogout: () => void;
  onLogin: () => void;
}

export default function MobileDrawer({
  open,
  onClose,
  user,
  palette,
  themeMode,
  langMode,
  currentPath,
  menuItems,
  onToggleTheme,
  onToggleLanguage,
  onLogout,
  onLogin,
}: MobileDrawerProps) {
  const { t } = useTranslation();

  return (
    <Drawer
      title={APP_NAME}
      placement="right"
      onClose={onClose}
      open={open}
      styles={{
        body: {
          backgroundColor: palette.bgLight,
          color: palette.text,
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        },
        header: {
          backgroundColor: palette.bgLight,
          borderBottom: `1px solid ${palette.border}`,
        },
      }}
    >
      <Menu
        mode="inline"
        selectedKeys={[currentPath]}
        items={menuItems}
        style={{
          backgroundColor: "transparent",
          borderRight: "none",
        }}
        onClick={(info) => {
          if (!info.keyPath || info.keyPath.length === 1) {
            onClose();
          }
        }}
      />

      <div
        style={{
          paddingTop: "16px",
          borderTop: `1px solid ${palette.border}`,
          display: "flex",
          flexDirection: "column",
          gap: "12px",
        }}
      >
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Text style={{ color: palette.textSecondary }}>{t("nav.theme")}</Text>
          <Button
            icon={themeMode === "dark" ? <SunOutlined /> : <MoonOutlined />}
            onClick={onToggleTheme}
          >
            {themeMode === "dark" ? t("nav.lightMode") : t("nav.darkMode")}
          </Button>
        </div>

        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Text style={{ color: palette.textSecondary }}>
            {t("nav.language")}
          </Text>
          <Button icon={<GlobalOutlined />} onClick={onToggleLanguage}>
            {String(langMode) === "vi" ? t("nav.vietnamese") : t("nav.english")}
          </Button>
        </div>

        {/* MOBILE: Show logout button if user is logged in, otherwise show login button */}
        {user ? (
          <Button
            danger
            block
            icon={<LogoutOutlined />}
            onClick={onLogout}
            style={{ marginTop: "8px" }}
          >
            {t("nav.logout")}
          </Button>
        ) : (
          <Button
            type="primary"
            block
            onClick={onLogin}
            style={{ marginTop: "8px" }}
          >
            {t("nav.login")}
          </Button>
        )}
      </div>
    </Drawer>
  );
}
