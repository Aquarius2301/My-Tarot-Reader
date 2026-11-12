import { useTranslation } from "react-i18next";
import { Button, Layout } from "antd";
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  MoonOutlined,
  SunOutlined,
} from "@ant-design/icons";

import { Coin } from "@/components";
import type { LanguageMode, LayoutVariant, Palette } from "@/constants";

const { Header } = Layout;

type LayoutHeaderProps = {
  isMobile: boolean;
  collapsed: boolean;
  title: string;
  variant: LayoutVariant;
  isDark: boolean;
  language: LanguageMode;
  palette: Palette;
  redCoins?: number;
  whiteCoins?: number;
  onToggleMenu: () => void;
  onLogin: () => void;
  onToggleLanguage: () => void;
  onToggleTheme: () => void;
};

export function LayoutHeader({
  isMobile,
  collapsed,
  title,
  variant,
  isDark,
  language,
  palette,
  redCoins,
  whiteCoins,
  onToggleMenu,
  onLogin,
  onToggleLanguage,
  onToggleTheme,
}: LayoutHeaderProps) {
  const { t } = useTranslation();

  return (
    <Header
      className="layout-header"
      style={{
        background: palette.siderBg,
        borderBottom: `1px solid ${palette.accentGlow}`,
      }}
    >
      <Button
        type="text"
        aria-label={isMobile ? "Expand menu" : "Collapse menu"}
        icon={
          isMobile || collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />
        }
        onClick={onToggleMenu}
      />
      <span className="layout-header__title" style={{ color: palette.primary }}>
        {title}
      </span>
      <div className="layout-header__spacer" />

      {variant == "register" && (
        <>
          <Coin type="white" number={whiteCoins} />
          <Coin type="red" number={redCoins} />
        </>
      )}

      {variant == "guest" && (
        <Button
          type="text"
          aria-label={t("component.mainLayout.login")}
          onClick={onLogin}
          style={{
            color: palette.siderBg,
            background: palette.primary,
          }}
        >
          {t("component.mainLayout.login")}
        </Button>
      )}

      <Button
        type="text"
        aria-label={t("component.mainLayout.language")}
        title={t("component.mainLayout.language")}
        onClick={onToggleLanguage}
        style={{
          minWidth: 44,
          color: palette.primary,
          background: palette.accentGlow,
        }}
      >
        {language === "vi" ? "VI" : "EN"}
      </Button>
      <Button
        type="text"
        shape="circle"
        aria-label={
          isDark
            ? t("component.mainLayout.darkMode")
            : t("component.mainLayout.lightMode")
        }
        icon={isDark ? <SunOutlined /> : <MoonOutlined />}
        onClick={onToggleTheme}
        style={{
          color: palette.primary,
          background: palette.accentGlow,
        }}
      />
    </Header>
  );
}
