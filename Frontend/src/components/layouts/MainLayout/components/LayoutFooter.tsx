import { Layout } from "antd";
import { APP_NAME } from "@/constants";
import type { getPaletteByRole } from "@/constants";

export type Palette = ReturnType<typeof getPaletteByRole>;

export interface LayoutFooterProps {
  palette: Palette;
}

const { Footer } = Layout;

export default function LayoutFooter({ palette }: LayoutFooterProps) {
  return (
    <Footer
      style={{
        textAlign: "center",
        backgroundColor: palette.bgLight,
        borderTop: `1px solid ${palette.border}`,
        color: palette.textSecondary,
      }}
    >
      {APP_NAME} ©{new Date().getFullYear()}
    </Footer>
  );
}
