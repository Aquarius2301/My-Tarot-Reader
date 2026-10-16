import {
  Avatar,
  Button,
  Dropdown,
  Image,
  Menu,
  Space,
  Tag,
  Typography,
  Layout,
  type MenuProps,
} from "antd";
import {
  GlobalOutlined,
  LogoutOutlined,
  MoonOutlined,
  SunOutlined,
  UserOutlined,
} from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import {
  APP_NAME,
  type LanguageMode,
  type getPaletteByRole,
} from "@/constants";
import type { GetCurrentUserResult } from "@/types";
import { WEB_URL } from "@/routes";
import { useNavigate } from "react-router-dom";

type Palette = ReturnType<typeof getPaletteByRole>;

const { Header } = Layout;
const { Text } = Typography;

export interface AdminHeaderProps {
  isMobile: boolean;
  user?: GetCurrentUserResult;
  palette: Palette;
  themeMode: "dark" | "light";
  langMode: LanguageMode;
  currentPath: string;
  menuItems: MenuProps["items"];
  onToggleTheme: () => void;
  onToggleLanguage: () => void;
  onLogout: () => void;
}

export default function AdminHeader({
  isMobile,
  user,
  palette,
  themeMode,
  langMode,
  currentPath,
  menuItems,
  onToggleTheme,
  onToggleLanguage,
  onLogout,
}: AdminHeaderProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();

  const userDropdownItems: MenuProps["items"] = [
    {
      key: "user-info",
      label: (
        <div style={{ padding: "4px 8px" }}>
          <Text strong style={{ display: "block" }}>
            {user?.fullName}
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
      onClick: onLogout,
    },
  ];

  return (
    <Header
      style={{
        position: "sticky",
        top: 0,
        zIndex: 1000,
        display: "flex",
        alignItems: "center",
        justifyContent: "space-between",
        gap: "12px",
        padding: isMobile ? "0 16px" : "0 32px",
        backgroundColor: palette.bgLight,
        borderBottom: `1px solid ${palette.border}`,
      }}
    >
      {/* Logo & APP_NAME & Admin badge */}
      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "12px",
          cursor: "pointer",
          flexShrink: 0,
        }}
        onClick={() => navigate(WEB_URL.admin)}
      >
        <Image src="/logo.png" preview={false} width={30} />
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
        <Tag
          style={{
            marginInlineStart: 0,
            backgroundColor: palette.primary,
            borderColor: palette.primary,
            color: palette.textOnPrimary,
          }}
        >
          {t("page.admin.title")}
        </Tag>
      </div>

      {/* Desktop Navigation */}
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
      <Space size="middle" style={{ flexShrink: 0 }}>
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

        <Dropdown menu={{ items: userDropdownItems }} placement="bottomRight" arrow>
          <Avatar
            src={user?.picture}
            icon={<UserOutlined />}
            style={{ backgroundColor: palette.primary, cursor: "pointer" }}
          />
        </Dropdown>
      </Space>
    </Header>
  );
}
