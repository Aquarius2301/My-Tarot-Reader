import type { CSSProperties } from "react";
import { Space, Typography, Tooltip } from "antd";
import { useTranslation } from "react-i18next";
import { COIN_COLORS } from "@/constants";
import type { Palette } from "./LayoutFooter";

const { Text } = Typography;

export interface CoinBadgeProps {
  whiteCoin: number;
  redCoin: number;
  palette: Palette;
  isMobile: boolean;
  themeMode: "dark" | "light";
}

/**
 * A coin-shaped SVG icon (outer rim + inner ring) that reads as a coin ("đồng xu").
 * The color is taken from the theme's COIN_COLORS so the two balances stay
 * visually distinct in both light and dark mode.
 */
function CoinIcon({ color, size }: { color: string; size: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden="true"
      style={{ flexShrink: 0 }}
    >
      <circle cx="12" cy="12" r="10" stroke={color} strokeWidth="2.2" />
      <circle
        cx="12"
        cy="12"
        r="6.2"
        stroke={color}
        strokeWidth="1.6"
        opacity="0.55"
      />
    </svg>
  );
}

/**
 * Displays the authenticated user's wallet balances (white + red coins) as two
 * compact coin indicators (icon + formatted count) for the header.
 * Only rendered when a user is signed in.
 */
export default function CoinBadge({
  whiteCoin,
  redCoin,
  palette,
  isMobile,
  themeMode,
}: CoinBadgeProps) {
  const { t } = useTranslation();

  const formatCoins = (value: number) => value.toLocaleString("vi-VN");

  // White coin is bright/light on dark surfaces but blends into a light header,
  // so on light mode it becomes a silver-grey coin (still distinct from red).
  const whiteCoinColor =
    themeMode === "dark" ? COIN_COLORS.white : COIN_COLORS.whiteLight;

  const coinStyle = (): CSSProperties => ({
    display: "inline-flex",
    alignItems: "center",
    gap: isMobile ? 4 : 7,
    cursor: "default",
    whiteSpace: "nowrap",
  });

  const coinTextStyle = (): CSSProperties => ({
    fontSize: isMobile ? 12 : 14,
    fontWeight: 600,
    color: palette.text,
    lineHeight: 1,
  });

  const iconSize = isMobile ? 13 : 16;

  return (
    <Space size={isMobile ? 8 : 14}>
      <Tooltip title={t("component.mainLayout.whiteCoin")}>
        <span style={coinStyle()}>
          <CoinIcon color={whiteCoinColor} size={iconSize} />
          <Text style={coinTextStyle()}>{formatCoins(whiteCoin)}</Text>
        </span>
      </Tooltip>
      <Tooltip title={t("component.mainLayout.redCoin")}>
        <span style={coinStyle()}>
          <CoinIcon color={COIN_COLORS.red} size={iconSize} />
          <Text style={coinTextStyle()}>{formatCoins(redCoin)}</Text>
        </span>
      </Tooltip>
    </Space>
  );
}
