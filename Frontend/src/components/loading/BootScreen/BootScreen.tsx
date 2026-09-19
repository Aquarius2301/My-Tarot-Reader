import { ConfigProvider, Spin, Typography } from "antd";
import { APP_NAME, getPaletteByRole, getThemeByRole } from "@/constants";
import { useThemeStore } from "@/hooks/stores";
import { useTranslation } from "react-i18next";

const { Text } = Typography;

/**
 * Full-screen branded splash shown while the app resolves the auth state on
 * cold boot. Uses the guest palette for the current theme mode (the role is
 * not known yet), so it never flashes a wrong header chrome.
 */
export default function BootScreen() {
  const mode = useThemeStore((s) => s.mode);
  const palette = getPaletteByRole(undefined, mode);
  const { t } = useTranslation();

  return (
    <ConfigProvider theme={getThemeByRole(undefined, mode)}>
      <div
        style={{
          minHeight: "100vh",
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          justifyContent: "center",
          gap: 24,
          backgroundColor: palette.bgLight,
        }}
      >
        <Text
          strong
          style={{
            fontSize: "1.5rem",
            color: palette.text,
            margin: 0,
            letterSpacing: "0.5px",
            marginBottom: 8,
          }}
        >
          {APP_NAME}
        </Text>

        <Spin fullscreen />
      </div>
    </ConfigProvider>
  );
}
