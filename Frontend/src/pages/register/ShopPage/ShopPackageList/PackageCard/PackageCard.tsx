import { Button, Card, Flex, Tag, Typography, theme } from "antd";
import { useTranslation } from "react-i18next";
import type { GetPackagesItem } from "@/types";
import { formatVnd } from "@/utils";

const { Text } = Typography;

export interface PackageCardProps {
  pkg: GetPackagesItem;
  loading: boolean;
  disabled: boolean;
  onBuy: () => void;
}

/**
 * One purchasable package rendered as a self-contained card: the granted red
 * coins (and bonus white coins) on the top left, the discount badge on the top
 * right, then the prices and a full-width buy button at the bottom.
 */
export function PackageCard({
  pkg,
  loading,
  disabled,
  onBuy,
}: PackageCardProps) {
  const { t } = useTranslation();
  const { token } = theme.useToken();

  const hasDiscount = pkg.oldPrice > pkg.price;
  const discountPercent = hasDiscount
    ? Math.round((1 - pkg.price / pkg.oldPrice) * 100)
    : 0;

  return (
    <Card style={{ height: "100%" }}>
      <Flex vertical gap={token.marginMD} style={{ height: "100%" }}>
        <Flex justify="space-between" align="flex-start" gap={token.marginSM}>
          <Flex align="baseline" gap={token.marginXS}>
            <Text strong style={{ fontSize: 28, lineHeight: 1 }}>
              {pkg.redCoins.toLocaleString("vi-VN")}
            </Text>
            <Text>{t("page.shop.packageRedCoins")}</Text>
          </Flex>

          {hasDiscount && (
            <Tag color="gold" style={{ margin: 0 }}>
              {t("page.shop.discountBadge", { percent: discountPercent })}
            </Tag>
          )}
        </Flex>

        {pkg.whiteCoins > 0 && (
          <Tag color="gold" style={{ margin: 0, alignSelf: "flex-start" }}>
            +{pkg.whiteCoins} {t("page.shop.packageWhiteCoins")}
          </Tag>
        )}

        <div style={{ flex: 1 }} />

        <Flex justify="flex-end" align="baseline" gap={token.marginXS} wrap>
          {hasDiscount && (
            <Text
              delete
              type="secondary"
              aria-label={t("page.shop.oldPriceLabel")}
            >
              {formatVnd(pkg.oldPrice)}
            </Text>
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
        </Flex>

        <Button
          type="primary"
          size="large"
          block
          loading={loading}
          disabled={disabled}
          onClick={onBuy}
        >
          {t("page.shop.buyButton")}
        </Button>
      </Flex>
    </Card>
  );
}
