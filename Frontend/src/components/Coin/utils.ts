import { coinColors } from "@/constants";
import type { CoinColors } from "./types";

export const getColor = (type: CoinColors, mode: "dark" | "light"): string => {
  if (type === "red") {
    return mode === "dark" ? coinColors.red : coinColors.redDark;
  }

  return mode === "dark" ? coinColors.white : coinColors.whiteLight;
};
