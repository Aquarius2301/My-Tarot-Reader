import { Button, Divider, Grid, Space, Tag, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import { CoinIcon } from "@/components";
import type { GetPackagesItem } from "@/types";
import { formatVnd } from "@/utils";

const { Text } = Typography;
const { useBreakpoint } = Grid;

export interface PackageRowProps {
  pkg: GetPackagesItem;
  loading: boolean;
  disabled: boolean;
  onBuy: () => void;
}

/**
 * One purchasable package rendered as a list row: the red coins granted (plus
 * bonus white coins when any) on the left, then the old/new price with the
 * % off badge between them, and the buy button on the right. Below the `md`
 * breakpoint the row stacks into two lines: coins first, price + button after.
 */
export default function PackageRow({
  pkg,
  loading,
  disabled,
  onBuy,
}: PackageRowProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  const hasDiscount = pkg.oldPrice > pkg.price;
  const discountPercent = hasDiscount
    ? Math.round((1 - pkg.price / pkg.oldPrice) * 100)
    : 0;

  const coins = (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: isMobile ? "flex-start" : undefined,
        gap: token.marginXS,
      }}
    >
      <Space size={8} align="center">
        <CoinIcon variant="red" size={22} />
        <Text strong style={{ fontSize: 24, lineHeight: 1 }}>
          {pkg.redCoins.toLocaleString("vi-VN")}
        </Text>
        <Text type="secondary">{t("page.shop.packageRedCoins")}</Text>
      </Space>
      {pkg.whiteCoins > 0 && (
        <Space size={6} align="center">
          <CoinIcon variant="white" size={14} />
          <Tag color="gold" style={{ margin: 0 }}>
            +{pkg.whiteCoins} {t("page.shop.packageWhiteCoins")}
          </Tag>
        </Space>
      )}
    </div>
  );

  const price = (
    <Space size={8} align="center" wrap>
      {hasDiscount && (
        <Text
          delete
          type="secondary"
          aria-label={t("page.shop.oldPriceLabel")}
        >
          {formatVnd(pkg.oldPrice)}
        </Text>
      )}
      {hasDiscount && (
        <Tag color="gold" style={{ margin: 0 }}>
          {t("page.shop.discountBadge", { percent: discountPercent })}
        </Tag>
      )}
      <Text
        strong
        style={{
          fontSize: 20,
          lineHeight: 1,
          color: hasDiscount ? token.colorPrimary : undefined,
        }}
      >
        {formatVnd(pkg.price)}
      </Text>
    </Space>
  );

  const buyButton = (
    <Button
      type="primary"
      loading={loading}
      disabled={disabled}
      onClick={onBuy}
      style={{ minWidth: 96 }}
    >
      {t("page.shop.buyButton")}
    </Button>
  );

  if (isMobile) {
    return (
      <div
        style={{
          display: "flex",
          flexDirection: "column",
          gap: token.marginSM,
          width: "100%",
        }}
      >
        {coins}
        <Divider plain style={{ margin: 0 }} />
        <div
          style={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            gap: token.marginSM,
          }}
        >
          {price}
          {buyButton}
        </div>
      </div>
    );
  }

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        gap: token.marginLG,
        flexWrap: "wrap",
        width: "100%",
      }}
    >
      <div style={{ flex: "1 1 220px" }}>{coins}</div>
      <Divider type="vertical" style={{ height: 44 }} />
      <div>{price}</div>
      <div style={{ marginLeft: "auto" }}>{buyButton}</div>
    </div>
  );
}
