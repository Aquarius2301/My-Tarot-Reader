import {
  Typography,
  Layout,
  Menu,
  Button,
  Space,
  Avatar,
  type MenuProps,
} from "antd";
import {
  SunOutlined,
  MoonOutlined,
  GlobalOutlined,
  MenuOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { APP_NAME, type LanguageMode } from "@/constants";
import type { UserResponse } from "@/types";
import type { Palette } from "./LayoutFooter";
import UserDropdown from "./UserDropdown";

const { Header } = Layout;
const { Text } = Typography;

export interface LayoutHeaderProps {
  isMobile: boolean;
  user?: UserResponse;
  palette: Palette;
  themeMode: "dark" | "light";
  langMode: LanguageMode;
  currentPath: string;
  menuItems: MenuProps["items"];
  userDropdownItems: MenuProps["items"];
  onToggleTheme: () => void;
  onToggleLanguage: () => void;
  onOpenMobileMenu: () => void;
  onLogin: () => void;
}

export default function LayoutHeader({
  isMobile,
  user,
  palette,
  themeMode,
  langMode,
  currentPath,
  menuItems,
  userDropdownItems,
  onToggleTheme,
  onToggleLanguage,
  onOpenMobileMenu,
  onLogin,
}: LayoutHeaderProps) {
  const { t } = useTranslation();

  return (
    <Header
      style={{
        position: "sticky",
        top: 0,
        zIndex: 1000,
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        padding: isMobile ? "0 16px" : "0 32px",
        backgroundColor: palette.bgLight,
        borderBottom: `1px solid ${palette.border}`,
      }}
    >
      {/* Logo & APP_NAME */}
      <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
        <Text
          strong
          style={{
            fontSize: "1.25rem",
            color: palette.text,
            margin: 0,
            letterSpacing: "0.5px",
          }}
        >
          {APP_NAME}
        </Text>
      </div>
      {/* Desktop Navigation */}
      {!isMobile && (
        <>
          <Menu
            mode="horizontal"
            selectedKeys={[currentPath]}
            items={menuItems}
            style={{
              flex: 1,
              minWidth: 0,
              justifyContent: "center",
              backgroundColor: "transparent",
              borderBottom: "none",
            }}
          />

          {/* Actions Right */}
          <Space size="middle">
            <Button
              type="text"
              icon={themeMode === "dark" ? <SunOutlined /> : <MoonOutlined />}
              onClick={onToggleTheme}
              style={{ color: palette.text }}
            />

            <Button
              type="text"
              icon={<GlobalOutlined />}
              onClick={onToggleLanguage}
              style={{ color: palette.text, fontWeight: 600 }}
            >
              {String(langMode).toUpperCase()}
            </Button>

            {user ? (
              /* DESKTOP: Wrap Avatar in a Dropdown containing user info & Sign out */
              <UserDropdown
                user={user}
                palette={palette}
                items={userDropdownItems}
              />
            ) : (
              <Button type="primary" onClick={onLogin}>
                {t("nav.login")}
              </Button>
            )}
          </Space>
        </>
      )}
      {/* Mobile Actions */}
      {isMobile && (
        <Space size="small">
          {user ? (
            <Avatar
              src={user?.picture}
              icon={<UserOutlined />}
              size="small"
              style={{ backgroundColor: palette.primary }}
            />
          ) : (
            <Button type="primary" size="small" onClick={onLogin}>
              {t("nav.login")}
            </Button>
          )}
          <Button
            type="text"
            icon={
              <MenuOutlined style={{ fontSize: "20px", color: palette.text }} />
            }
            onClick={onOpenMobileMenu}
          />
        </Space>
      )}
    </Header>
  );
}
