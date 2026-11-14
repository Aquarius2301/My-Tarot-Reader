import { Button, Flex, Menu } from "antd";
import type { LanguageMode, LayoutMenuItem, Palette } from "@/constants";
import { findActiveMenu } from "./utils";
import { MoonOutlined, SunOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";

type LayoutMenuProps = {
  items: LayoutMenuItem[];
  pathname: string;
  isDark: boolean;
  language: LanguageMode;
  palette: Palette;
  isButtonShow: boolean;
  onNavigate: (key: string) => void;
  onToggleLanguage: () => void;
  onToggleTheme: () => void;
};

export function LayoutMenu({
  items,
  pathname,
  isDark,
  language,
  palette,
  isButtonShow,
  onNavigate,
  onToggleLanguage,
  onToggleTheme,
}: LayoutMenuProps) {
  const { t } = useTranslation();

  const activeMenu = findActiveMenu(items, pathname);
  const selectedKeys = activeMenu ? [activeMenu.key] : [];

  return (
    <Flex vertical style={{ height: "100%" }} justify="space-between">
      <Menu
        key={pathname}
        mode="inline"
        theme={isDark ? "dark" : "light"}
        items={items}
        selectedKeys={selectedKeys}
        defaultOpenKeys={activeMenu?.openKeys}
        onClick={({ key }) => onNavigate(key)}
        style={{ background: "transparent", borderInlineEnd: 0 }}
      />
      {isButtonShow && (
        <Flex
          gap="middle"
          justify="center"
          align="center"
          style={{ width: "100%", marginBottom: 16 }}
        >
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
        </Flex>
      )}
    </Flex>
  );
}
