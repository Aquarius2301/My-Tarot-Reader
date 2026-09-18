import type { CSSProperties } from "react";
import { Space, Typography, Tooltip } from "antd";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";
import type { Palette } from "./LayoutFooter";

const { Text } = Typography;

export interface CoinBadgeProps {
  whiteCoin: number;
  redCoin: number;
  palette: Palette;
  isMobile: boolean;
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
}: CoinBadgeProps) {
  const { t } = useTranslation();

  const formatCoins = (value: number) => value.toLocaleString("vi-VN");

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
          <CoinIcon variant="white" size={iconSize} />
          <Text style={coinTextStyle()}>{formatCoins(whiteCoin)}</Text>
        </span>
      </Tooltip>
      <Tooltip title={t("component.mainLayout.redCoin")}>
        <span style={coinStyle()}>
          <CoinIcon variant="red" size={iconSize} />
          <Text style={coinTextStyle()}>{formatCoins(redCoin)}</Text>
        </span>
      </Tooltip>
    </Space>
  );
}