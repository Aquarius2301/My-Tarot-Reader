import { COIN_COLORS } from "@/constants";
import { useThemeStore } from "@/hooks/stores";

/**
 * A coin-shaped SVG icon (outer rim + inner ring) that reads as a coin.
 * The color is derived from the variant and the current theme mode: the white
 * coin turns silver-grey on light surfaces so it stays visible, and stays
 * bright on dark backgrounds, while the red coin keeps its color everywhere.
 */
export default function CoinIcon({
  variant = "white",
  size,
}: {
  variant?: "white" | "red";
  size: number;
}) {
  const themeMode = useThemeStore((s) => s.mode);

  const color =
    variant === "red"
      ? COIN_COLORS.red
      : themeMode === "dark"
        ? COIN_COLORS.white
        : COIN_COLORS.whiteLight;

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