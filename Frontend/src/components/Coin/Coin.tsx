import { Tooltip } from "antd";
import { DollarOutlined } from "@ant-design/icons";
import { useTranslation } from "react-i18next";
import { useThemeStore } from "@/hooks/stores";
import type { CoinProps } from "./types";
import { getColor } from "./utils";

export function Coin({ type, number = 0, isTooltip = true }: CoinProps) {
  const { t } = useTranslation();
  const mode = useThemeStore((state) => state.mode);

  return (
    <Tooltip
      title={t(`component.coin.${type}`)}
      placement="bottom"
      open={isTooltip ? undefined : false}
    >
      {number} <DollarOutlined style={{ color: getColor(type, mode) }} />
    </Tooltip>
  );
}
