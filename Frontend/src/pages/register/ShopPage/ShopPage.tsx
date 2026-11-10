import { App, Col, Empty, Row } from "antd";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { AppTitle, ErrorView } from "@/components";
import { useCreateOrder, useGetPackages } from "@/hooks/api";
import { WEB_URL } from "@/routes";
import type { GetPackagesItem } from "@/types";
import { getErrorMessage, saveCheckoutPayload } from "@/utils";
import { PackageCard, ShopPageLoading } from "./components";

/**
 * Shop page: lists the purchasable red-coin packages as cards. Buying one
 * creates a PayOS order and hands the checkout payload over to the checkout
 * page, which polls the order until the payment settles.
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

  return (
    <div>
      <AppTitle
        title={t("page.shop.title")}
        subtitle={t("page.shop.subtitle")}
      />

      {isLoading ? (
        <ShopPageLoading />
      ) : data === undefined ? (
        <ErrorView type="server" onRetry={refetch} />
      ) : data.packages.length === 0 ? (
        <Empty description={t("page.shop.empty")} />
      ) : (
        <Row gutter={[16, 16]}>
          {data.packages.map((pkg) => (
            <Col xs={24} sm={12} lg={8} key={pkg.code}>
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
