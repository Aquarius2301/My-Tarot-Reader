import { ConfigProvider, Image, Spin } from "antd";
import { getPaletteByRole, getThemeByRole } from "@/constants";
import { useThemeStore } from "@/hooks/stores";

/**
 * Full-screen branded splash shown while the app resolves the auth state on
 * cold boot. Uses the guest palette for the current theme mode (the role is
 * not known yet), so it never flashes a wrong header chrome.
 */
export default function BootScreen() {
  const mode = useThemeStore((s) => s.mode);
  const palette = getPaletteByRole(undefined, mode);

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
        <Image src="./public/logo.png" preview={false} width={200} />

        <Spin fullscreen />
      </div>
    </ConfigProvider>
  );
}
