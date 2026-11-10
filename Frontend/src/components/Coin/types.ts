export type CoinColors = "red" | "white";

export type CoinProps = {
  type: CoinColors;
  number?: number;
  isTooltip?: boolean;
};
