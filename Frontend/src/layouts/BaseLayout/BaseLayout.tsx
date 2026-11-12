import { Suspense, useEffect, useState } from "react";
import { Outlet, useLocation, useNavigate } from "react-router-dom";
import { App as AntdApp, ConfigProvider, Grid, Layout } from "antd";
import {
  APP_NAME,
  getPalette,
  getTheme,
  type LayoutMenuItem,
  type LayoutVariant,
} from "@/constants";
import { useLanguageStore, useThemeStore } from "@/hooks/stores";
import { WEB_URL } from "@/routes";
import { useLogout } from "@/hooks/api";
import { UserFooter } from "./UserFooter";
import { LayoutMenu } from "./LayoutMenu";
import { Brand } from "./Brand";
import { LayoutHeader } from "./LayoutHeader";
import { MobileDrawer } from "./MobileDrawer";
import { LogoutModal } from "./LogoutModal";
import { AppLoading } from "@/components";

const { Sider, Content } = Layout;

export type BaseLayoutProps = {
  variant: LayoutVariant;
  title: string;
  items: LayoutMenuItem[];
  displayName?: string;
  picture?: string;
  redCoins?: number;
  whiteCoins?: number;
};

export function BaseLayout({
  variant,
  title,
  items,
  displayName,
  picture,
  redCoins,
  whiteCoins,
}: BaseLayoutProps) {
  const mode = useThemeStore((state) => state.mode);
  const toggleTheme = useThemeStore((state) => state.toggle);
  const language = useLanguageStore((state) => state.mode);
  const toggleLanguage = useLanguageStore((state) => state.toggle);
  const showUserFooter = variant !== "guest";
  const isDark = mode === "dark";
  const palette = getPalette(variant, mode);

  const screens = Grid.useBreakpoint();
  const isMobile = screens.xl === false;

  const [collapsed, setCollapsed] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [logoutModal, setLogoutModal] = useState(false);

  const location = useLocation();
  const navigate = useNavigate();

  const { mutate, isPending } = useLogout();

  useEffect(() => {
    document.documentElement.style.setProperty("--app-bg", palette.bgBase);
    document.documentElement.style.setProperty(
      "--app-primary",
      palette.primary,
    );
    document.documentElement.style.setProperty(
      "--app-card-bg",
      palette.siderBg,
    );
    document.documentElement.style.colorScheme = mode;
    document.body.style.backgroundColor = palette.bgBase;
    document.body.style.colorScheme = mode;

    document.title = title ? `${title} | ${APP_NAME}` : APP_NAME;
  }, [palette.bgBase, palette.primary, palette.siderBg, mode, title]);

  const handleNavigate = (key: string) => {
    navigate(key);
    setDrawerOpen(false);
  };

  const renderUserFooter = (showDetails: boolean) => (
    <UserFooter
      showDetails={showDetails}
      displayName={displayName}
      picture={picture}
      palette={palette}
      logoutModalOpen={logoutModal}
      onLogout={() => setLogoutModal(true)}
    />
  );

  const menu = (
    <LayoutMenu
      items={items}
      pathname={location.pathname}
      isDark={isDark}
      onNavigate={handleNavigate}
    />
  );

  return (
    <ConfigProvider theme={getTheme(isDark, palette)}>
      <AntdApp>
        <Layout style={{ minHeight: "100vh", background: palette.bgBase }}>
          {!isMobile && (
            <Sider
              collapsible
              collapsed={collapsed}
              trigger={null}
              width={248}
              collapsedWidth={80}
              theme={isDark ? "dark" : "light"}
              style={{
                background: palette.siderBg,
                position: "sticky",
                top: 0,
                height: "100vh",
                overflow: "hidden",
                borderInlineEnd: `1px solid ${palette.accentGlow}`,
              }}
            >
              <div className="layout-sider__inner">
                <div className="layout-sider__header">
                  <Brand
                    showText={!collapsed}
                    isMobile={isMobile}
                    palette={palette}
                  />
                </div>
                <div className="layout-sider__menu">{menu}</div>
                {showUserFooter && renderUserFooter(!collapsed)}
              </div>
            </Sider>
          )}

          <Layout style={{ background: palette.bgBase }}>
            <LayoutHeader
              isMobile={isMobile}
              collapsed={collapsed}
              title={title}
              variant={variant}
              isDark={isDark}
              language={language}
              palette={palette}
              redCoins={redCoins}
              whiteCoins={whiteCoins}
              onToggleMenu={() =>
                isMobile ? setDrawerOpen(true) : setCollapsed((value) => !value)
              }
              onLogin={() => navigate(WEB_URL.login)}
              onToggleLanguage={toggleLanguage}
              onToggleTheme={toggleTheme}
            />

            <Content className="layout-content">
              <Suspense fallback={<AppLoading />}>
                <Outlet />
              </Suspense>
            </Content>
          </Layout>

          <MobileDrawer
            open={isMobile && drawerOpen}
            onClose={() => setDrawerOpen(false)}
            brand={<Brand showText isMobile={isMobile} palette={palette} />}
            menu={menu}
            userFooter={showUserFooter ? renderUserFooter(true) : undefined}
          />
        </Layout>

        <LogoutModal
          open={logoutModal}
          onClose={() => setLogoutModal(false)}
          onLogout={() => {
            mutate(undefined, {
              onSuccess: () => {
                navigate(WEB_URL.guestHome, { replace: true });
              },
            });
          }}
          loading={isPending}
        />
      </AntdApp>
    </ConfigProvider>
  );
}
