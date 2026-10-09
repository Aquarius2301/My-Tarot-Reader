import { App, Col, Empty, Row, Spin, Typography } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { ErrorComponent } from "@/components";
import { useCreateOrder, useGetPackages } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import type { GetPackagesItem } from "@/types";
import { getErrorMessage, saveCheckoutPayload } from "@/utils";
import { PackageCard } from "./components";

const { Title, Text } = Typography;

/**
 * Shop page: lists the purchasable red-coin packages. Buying one creates a
 * PayOS order and hands the checkout payload over to the checkout page, which
 * polls the order until the payment settles.
 */
export default function ShopPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { message } = App.useApp();
  const { data, isLoading, refetch } = useGetPackages();
  const { mutate, isPending, variables } = useCreateOrder();

  const handleBuy = (pkg: GetPackagesItem) => {
    mutate(
      { packageCode: pkg.code },
      {
        onSuccess: (order) => {
          saveCheckoutPayload(order.orderId, {
            checkoutUrl: order.checkoutUrl,
            qrCode: order.qrCode,
          });
          navigate(`${WEB_URL.shopCheckout}/${order.orderId}`, {
            state: order,
          });
        },
        onError: (error) => message.error(getErrorMessage(error)),
      },
    );
  };

  if (isLoading) {
    return <Spin fullscreen />;
  }

  if (data === undefined) {
    return <ErrorComponent type="server" onRetry={refetch} />;
  }

  return (
    <div style={{ maxWidth: 960, margin: "0 auto" }}>
      <Title level={2} style={{ textAlign: "center", marginBottom: 4 }}>
        {t("page.shop.title")}
      </Title>
      <Text
        type="secondary"
        style={{ display: "block", textAlign: "center", marginBottom: 24 }}
      >
        {t("page.shop.subtitle")}
      </Text>

      {data.packages.length === 0 ? (
        <Empty description={t("page.shop.empty")} />
      ) : (
        <Row gutter={[16, 16]}>
          {data.packages.map((pkg) => (
            <Col key={pkg.code} xs={24} sm={8}>
              <PackageCard
                pkg={pkg}
                loading={isPending && variables?.packageCode === pkg.code}
                disabled={isPending}
                onBuy={() => handleBuy(pkg)}
              />
            </Col>
          ))}
        </Row>
      )}
    </div>
  );
}
